using Hi_Trade.Services.Interfaces;
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
            if(endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() == null)
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if(token != null)
                {
                    var blacklisted = await userService.CheckBlacklisted(token);
                    if(blacklisted)
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
