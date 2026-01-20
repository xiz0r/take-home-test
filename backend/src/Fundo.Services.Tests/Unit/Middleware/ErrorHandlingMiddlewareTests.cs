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
    [Theory]
    [MemberData(nameof(ExceptionTestCases))]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsExpectedStatusAndMessage(
        Exception exception,
        HttpStatusCode expectedStatusCode,
        string expectedError,
        string traceId)
    {
        // Arrange
        var context = CreateHttpContext(traceId);
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)expectedStatusCode);

        var payload = await ReadPayloadAsync(context);
        payload.Error.Should().Be(expectedError);
        payload.TraceId.Should().Be(traceId);
    }

    public static IEnumerable<object[]> ExceptionTestCases =>
        new List<object[]>
        {
            new object[]
            {
                new ArgumentException("invalid data"),
                HttpStatusCode.BadRequest,
                "invalid data",
                "trace-arg"
            },
            new object[]
            {
                new KeyNotFoundException("missing"),
                HttpStatusCode.NotFound,
                "missing",
                "trace-missing"
            },
            new object[]
            {
                new InvalidDataException("boom"),
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                "trace-500"
            }
        };

    [Fact]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_Rethrows()
    {
        // Arrange
        var context = CreateStartedHttpContext("trace-started");
        var middleware = CreateMiddleware(async httpContext =>
        {
            await httpContext.Response.StartAsync();
            throw new InvalidOperationException("started");
        });

        // Act
        var action = () => middleware.InvokeAsync(context);

        // Assert
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
