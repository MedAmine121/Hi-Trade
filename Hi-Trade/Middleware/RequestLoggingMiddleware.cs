using Hi_Trade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade
{
    public class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, IUserService userService) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var endpoint = context.GetEndpoint();
            bool requiresAuth = endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null
                && endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() == null;
            if (requiresAuth)
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var blacklisted = await userService.CheckBlacklisted(token);
                    if (blacklisted)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                }
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            string message = $"Request: {context.Request.Method} {context.Request.Path} - ";
            if (string.IsNullOrEmpty(body))
            {
                body = context.Request.QueryString.Value;
            }
            context.Request.Body.Position = 0;

            await next(context);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            message += $" Response Status: {context.Response.StatusCode} - Time Taken: {ts.TotalMilliseconds} ms \n {body}";
            logger.LogInformation(message);
        }
    }
}
