using System.Diagnostics;
using System.Text.Json;
using CaManagement.Api.Services.Exceptions;

namespace CaManagement.Api.Middlewares;

/// <summary>Maps domain exceptions and unhandled errors to RFC 7807 <see cref="ProblemDetails"/>.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled pipeline exception.");
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = ex switch
        {
            NotFoundException n => (HttpStatusCode.NotFound, "Resource not found", n.Message),
            ConflictException c => (HttpStatusCode.Conflict, "Conflict", c.Message),
            DomainValidationException d => (HttpStatusCode.BadRequest, "Validation failed", d.Message),
            _ => (HttpStatusCode.InternalServerError, "Unexpected error", "An unexpected error occurred.")
        };

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)status}"
        };
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
