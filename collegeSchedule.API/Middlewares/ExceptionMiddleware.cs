using System.Net;
using System.Text.Json;

namespace collegeSchedule.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // Обрабатываем ошибку и отправляем JSON-ответ
                await HandleException(context, ex);
            }
        }

        private static Task HandleException(HttpContext context, Exception ex)
        {
            // Определяем HTTP статус в зависимости от типа исключения
            var statusCode = ex switch
            {
                ArgumentOutOfRangeException => HttpStatusCode.BadRequest, // 400
                ArgumentException => HttpStatusCode.BadRequest,       // 400
                KeyNotFoundException => HttpStatusCode.NotFound,      // 404
                _ => HttpStatusCode.InternalServerError             // 500
            };

            var response = new { error = ex.Message };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
