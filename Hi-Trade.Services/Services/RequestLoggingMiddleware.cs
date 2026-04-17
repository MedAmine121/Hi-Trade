using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Services.Services
{
    public class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, RequestDelegate _next)
    {
        public async Task Invoke(HttpContext context)
        {
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

            await _next(context);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            message += $" Response Status: {context.Response.StatusCode} - Time Taken: {ts.TotalMilliseconds} ms \n {body}";
            logger.LogInformation(message);
        }
    }
}
