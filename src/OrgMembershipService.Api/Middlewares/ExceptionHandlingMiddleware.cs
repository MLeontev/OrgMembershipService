using FluentValidation;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Api.Middlewares;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            await WriteError(
                context,
                StatusCodes.Status400BadRequest,
                new ErrorResponse(
                    Code: "VALIDATION_ERROR",
                    Description: "Ошибка валидации",
                    Errors: errors));
        }
        catch (AppException ex)
        {
            await WriteError(
                context,
                ex.StatusCode,
                new ErrorResponse(
                    Code: ex.Code,
                    Description: ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            
            await WriteError(
                context,
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(
                    Code: "INTERNAL_ERROR",
                    Description: "Непредвиденная ошибка"));
        }
    }

    private async Task WriteError(HttpContext context, int statusCode, ErrorResponse response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}