using System.Net;

namespace Fundo.Applications.WebApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            var (statusCode, message) = MapException(ex);

            if (context.Response.HasStarted)
            {
                this.logger.LogWarning(
                    ex,
                    "Response already started for {Method} {Path} TraceId {TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    traceId);
                throw;
            }

            if (statusCode == (int)HttpStatusCode.InternalServerError)
            {
                this.logger.LogError(
                    ex,
                    "Unhandled exception for {Method} {Path} TraceId {TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    traceId);
            }
            else
            {
                this.logger.LogWarning(
                    ex,
                    "Request failed with {StatusCode} for {Method} {Path} TraceId {TraceId}",
                    statusCode,
                    context.Request.Method,
                    context.Request.Path.Value,
                    traceId);
            }

            await WriteErrorResponseAsync(context, statusCode, message, traceId);
        }
    }

    private static (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
    }

    private static Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        string traceId)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse(message, traceId);
        return context.Response.WriteAsJsonAsync(response);
    }

    private sealed record ErrorResponse(string Error, string TraceId);
}
