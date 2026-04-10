namespace AuthService.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var start = DateTime.UtcNow;

            try
            {
                await _next(context);

                var elapsed = DateTime.UtcNow - start;

                _logger.LogInformation("HTTP {Method} {Path} from {IP} responded {StatusCode} in {Elapsed} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }
        }
    }
}