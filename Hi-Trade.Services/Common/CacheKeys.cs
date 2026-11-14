using System;

namespace Hi_Trade.Services.Common
{
    public static class CacheKeys
    {
        // Prefixes
        public const string UserPrefix = "user:";
        public const string AssetsPrefix = "general:assets";
        public const string GeneralPrefix = "general:";
        public const string BlacklistPrefix = "blacklist:";

        // General Data Keys
        public const string EnabledAssets = "general:assets:enabled";
        public const string AdminDashboardStats = "general:admin:dashboard-stats";

        // Expirations
        public static readonly TimeSpan DefaultUserExpiry = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan DefaultPortfoliosExpiry = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan DefaultAssetsExpiry = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultDashboardStatsExpiry = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan DefaultGeneralExpiry = TimeSpan.FromMinutes(10);

        // Key Builders
        public static string UserProfile(string email) =>
            $"{UserPrefix}profile:{(email ?? string.Empty).Trim().ToLowerInvariant()}";

        public static string UserPortfolios(string email) =>
            $"{UserPrefix}portfolios:{(email ?? string.Empty).Trim().ToLowerInvariant()}";

        public static string AllAssets(string? search, bool? enabled) =>
            $"{AssetsPrefix}:all:search={search?.Trim().ToLowerInvariant()}:enabled={enabled}";

        public static string TokenBlacklist(string jti) =>
            $"{BlacklistPrefix}{jti}";
    }
}
