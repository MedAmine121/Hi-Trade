using Hi_Trade.Models.Common;
using Hi_Trade.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class RedisCacheService : ICacheService, IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RedisOptions _redisOptions;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private IConnectionMultiplexer? _connectionMultiplexer;
        private readonly bool _ownsMultiplexer;
        private bool _disposed;

        public RedisCacheService(
            IOptions<RedisOptions> redisOptions,
            ILogger<RedisCacheService> logger,
            IConnectionMultiplexer? connectionMultiplexer = null)
        {
            _redisOptions = redisOptions?.Value ?? new RedisOptions();
            _logger = logger;
            if (connectionMultiplexer != null)
            {
                _connectionMultiplexer = connectionMultiplexer;
                _ownsMultiplexer = false;
            }
            else
            {
                _ownsMultiplexer = true;
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            try
            {
                var db = await GetDatabaseAsync();
                if (db == null)
                {
                    return default;
                }

                var redisValue = await db.StringGetAsync(key);
                if (!redisValue.HasValue || redisValue.IsNullOrEmpty)
                {
                    return default;
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)redisValue.ToString();
                }

                return JsonSerializer.Deserialize<T>(redisValue.ToString(), JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve cache key {Key} from Redis.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return;
            }

            try
            {
                var db = await GetDatabaseAsync();
                if (db == null)
                {
                    return;
                }

                string serialized = value is string strVal
                    ? strVal
                    : JsonSerializer.Serialize(value, JsonOptions);

                await db.StringSetAsync(key, serialized, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache key {Key} in Redis.", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            try
            {
                var db = await GetDatabaseAsync();
                if (db == null)
                {
                    return;
                }

                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete cache key {Key} from Redis.", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            try
            {
                var db = await GetDatabaseAsync();
                if (db == null || _connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
                {
                    return;
                }

                var endpoints = _connectionMultiplexer.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var server = _connectionMultiplexer.GetServer(endpoint);
                        if (server.IsConnected)
                        {
                            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                            if (keys.Length > 0)
                            {
                                await db.KeyDeleteAsync(keys);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to scan keys by prefix {Prefix} on endpoint {Endpoint}.", prefix, endpoint);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove keys by prefix {Prefix} from Redis.", prefix);
            }
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            var cached = await GetAsync<T>(key, ct);
            if (cached != null)
            {
                return cached;
            }

            var freshValue = await factory();
            if (freshValue != null)
            {
                await SetAsync(key, freshValue, expiry, ct);
            }

            return freshValue;
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            try
            {
                var db = await GetDatabaseAsync();
                if (db == null)
                {
                    return false;
                }

                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check existence for cache key {Key} in Redis.", key);
                return false;
            }
        }

        private async Task<IDatabase?> GetDatabaseAsync()
        {
            if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            {
                return _connectionMultiplexer.GetDatabase();
            }

            if (string.IsNullOrWhiteSpace(_redisOptions.EndpointUrl))
            {
                return null;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    return _connectionMultiplexer.GetDatabase();
                }

                var port = _redisOptions.Port > 0 ? _redisOptions.Port : 6379;
                var config = new ConfigurationOptions
                {
                    EndPoints = { { _redisOptions.EndpointUrl, port } },
                    User = string.IsNullOrWhiteSpace(_redisOptions.User) ? null : _redisOptions.User,
                    Password = string.IsNullOrWhiteSpace(_redisOptions.Password) ? null : _redisOptions.Password,
                    AbortOnConnectFail = false,
                    ConnectTimeout = 3000,
                    SyncTimeout = 3000
                };

                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(config);
                return _connectionMultiplexer.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to Redis at {EndpointUrl}:{Port}. Caching will be bypassed.",
                    _redisOptions.EndpointUrl, _redisOptions.Port);
                return null;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_ownsMultiplexer)
            {
                _connectionMultiplexer?.Dispose();
            }
            _connectionLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
