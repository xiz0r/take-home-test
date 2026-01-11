using System.Net;
using System.Text.Json;
using FluentAssertions;
using Fundo.Applications.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fundo.Services.Tests.Unit.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenArgumentException_ReturnsBadRequest()
    {
        var context = CreateHttpContext("trace-arg");
        var middleware = CreateMiddleware(_ => throw new ArgumentException("invalid data"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var payload = await ReadPayloadAsync(context);
        payload.Error.Should().Be("invalid data");
        payload.TraceId.Should().Be("trace-arg");
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_ReturnsNotFound()
    {
        var context = CreateHttpContext("trace-missing");
        var middleware = CreateMiddleware(_ => throw new KeyNotFoundException("missing"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var payload = await ReadPayloadAsync(context);
        payload.Error.Should().Be("missing");
        payload.TraceId.Should().Be("trace-missing");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedException_ReturnsInternalServerError()
    {
        var context = CreateHttpContext("trace-500");
        var middleware = CreateMiddleware(_ => throw new InvalidDataException("boom"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var payload = await ReadPayloadAsync(context);
        payload.Error.Should().Be("An unexpected error occurred.");
        payload.TraceId.Should().Be("trace-500");
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_Rethrows()
    {
        var context = CreateStartedHttpContext("trace-started");
        var middleware = CreateMiddleware(async httpContext =>
        {
            await httpContext.Response.StartAsync();
            throw new InvalidOperationException("started");
        });

        var action = () => middleware.InvokeAsync(context);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("started");
    }

    private static ErrorHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<ErrorHandlingMiddleware>();
        return new ErrorHandlingMiddleware(next, logger);
    }

    private static DefaultHttpContext CreateHttpContext(string traceId)
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = traceId;
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/loans";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateStartedHttpContext(string traceId)
    {
        var context = CreateHttpContext(traceId);
        var responseFeature = new StartedResponseFeature(context.Response.Body);
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        return context;
    }

    private static async Task<(string Error, string TraceId)> ReadPayloadAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var json = await JsonDocument.ParseAsync(context.Response.Body);

        var root = json.RootElement;
        var error = ReadProperty(root, "error", "Error");
        var traceId = ReadProperty(root, "traceId", "TraceId");
        return (error, traceId);
    }

    private static string ReadProperty(JsonElement element, string camelName, string pascalName)
    {
        if (element.TryGetProperty(camelName, out var value) ||
            element.TryGetProperty(pascalName, out value))
        {
            return value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public StartedResponseFeature(Stream body)
        {
            Body = body;
        }

        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; }
        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
