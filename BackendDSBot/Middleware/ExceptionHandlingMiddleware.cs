using System.Net;
using Application.Services.Errors;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AppException ex)
        {
            await WriteProblem(ctx, ex.Error, ex, isDomain: false);
        }
        catch (DomainException ex)
        {
            await WriteProblem(ctx, new AppError(ErrorCodes.Conflict, ex.Message), ex, isDomain: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var err = new AppError("internal_error", "Unexpected error.");
            await WriteProblem(ctx, err, ex, isDomain: false, status: (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task WriteProblem(HttpContext ctx, AppError error, Exception ex, bool isDomain, int? status = null)
    {
        var httpStatus = status ?? error.Code switch
        {
            ErrorCodes.Validation => StatusCodes.Status400BadRequest,
            ErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ErrorCodes.Conflict => StatusCodes.Status409Conflict,
            ErrorCodes.InsufficientTickets => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        if (httpStatus >= 500)
            _logger.LogError(ex, "Error {Code}: {Message}", error.Code, error.Message);
        else
            _logger.LogWarning(ex, "Error {Code}: {Message}", error.Code, error.Message);

        var pd = new ProblemDetails
        {
            Status = httpStatus,
            Title = error.Code,
            Detail = error.Message,
            Instance = ctx.Request.Path
        };

        ctx.Response.StatusCode = httpStatus;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(pd);
    }
}
