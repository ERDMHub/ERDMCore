using ERDM.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ERDMCore.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case NotFoundException notFound:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = notFound.Message;
                    errorResponse.ErrorCode = "NOT_FOUND";
                    break;

                case BusinessRuleException businessRule:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = businessRule.Message;
                    errorResponse.ErrorCode = "BUSINESS_RULE_VIOLATION";
                    break;

                case ValidationException validation:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = validation.Message;
                    errorResponse.ErrorCode = "VALIDATION_ERROR";
                    errorResponse.Errors = validation.Errors;
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = "Unauthorized access";
                    errorResponse.ErrorCode = "UNAUTHORIZED";
                    break;

                case DomainException domain:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = domain.Message;
                    errorResponse.ErrorCode = "DOMAIN_ERROR";
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.StatusCode = response.StatusCode;
                    errorResponse.Message = "An error occurred while processing your request";
                    errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            var json = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(json);
        }
    }
}