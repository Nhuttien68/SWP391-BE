using EVMarketPlace.Repositories.Exception;
using System.Net;
using System.Text.Json;
using EVMarketPlace.Repositories.ResponseDTO;

namespace EVMarketPlace.API.Middleware
{
    public class GlobalException
    {
        private readonly RequestDelegate _next;

        public GlobalException(RequestDelegate next)
        {
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            BaseResponse errorResponse;
            switch (exception)
            {
                case NotFoundException notFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = notFoundException.Message,
                        Data = null
                    };
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "An unexpected error occurred.",
                        Data = null
                    };
                    break;
            }
            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }
}
