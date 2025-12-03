namespace MobyPark_api.Middleware
{
    public class ErrorHandelerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandelerMiddleware> _logger;
        public ErrorHandelerMiddleware(RequestDelegate next, ILogger<ErrorHandelerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("middleware was hit");
            await _next(context);
        }
    }
}
