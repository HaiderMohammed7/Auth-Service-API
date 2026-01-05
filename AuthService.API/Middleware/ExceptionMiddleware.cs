using System.Net;
using System.Text.Json;
using AuthService.Application.Exceptions;
using AuthService.Shared.Responses;

namespace AuthService.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                await WriteResponse(context, ex.Message);
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await WriteResponse(context, "Something went wrong");
            }

        }

        private static async Task WriteResponse(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var response = ApiResponse<string>.Fail(message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

    }
}