using System.Net;
namespace MobyPark_api.Middleware
{
    /// <summary>
    /// Middleware to handle errors. If an error occurs in the pipeline beyond this middleware the error will be loged. The exception is also returned in a http response.
    /// </summary>
    public class ErrorHandelerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandelerMiddleware> _logger;
        public ErrorHandelerMiddleware(RequestDelegate next, ILogger<ErrorHandelerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Method is invoked as part of DI pipeline
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        public async Task HandleException(HttpContext context, Exception ex)
        {
            LogException(ex);
            await FormatResponse(context, ex);
        }

        /// <summary>
        /// Defines how exceptions are logged
        /// </summary>
        /// <param name="ex">exception to log</param>
        public void LogException(Exception ex)
        {
            string errorMessage = "";

            errorMessage += ex.GetType().FullName;
            errorMessage += ": ";
            errorMessage += ex.Message;
            errorMessage += "\n";

            if (ex.StackTrace?.Length > 2000)
                errorMessage += ex.StackTrace?.Remove(2000);
            else
                errorMessage += ex.StackTrace;

            _logger.LogInformation(errorMessage);
        }

        /// <summary>
        /// Format the http response to be sent back to the client
        /// </summary>
        /// <param name="context">http context</param>
        /// <param name="ex">the excetion to format</param>
        /// <returns></returns>
        public async Task FormatResponse(HttpContext context, Exception ex)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            switch (ex.GetType())
            {
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync($"The server threw an {ex.GetType().Name}.\n{ex.Message}");
                    break;
            }
        }
    }
}
