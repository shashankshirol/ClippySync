using System.Text.Encodings.Web;
using ClippySync.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace ClippySync.Test;

public class DeviceKeyAuthenticationHandlerTests
{
    [Test]
    public async Task AuthenticateAsync_Succeeds_WhenHeaderMatchesMachineName()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Device-Key"] = Environment.MachineName.ToLowerInvariant();

        var handler = CreateHandler();
        await InitializeHandlerAsync(handler, context);

        var result = await handler.AuthenticateAsync();

        That(result.Succeeded, Is.True);
        That(result.Principal?.Identity?.IsAuthenticated, Is.True);
    }

    [Test]
    public async Task AuthenticateAsync_Fails_WhenHeaderMissing()
    {
        var context = new DefaultHttpContext();

        var handler = CreateHandler();
        await InitializeHandlerAsync(handler, context);

        var result = await handler.AuthenticateAsync();

        That(result.Succeeded, Is.False);
        That(result.Failure, Is.Not.Null);
        That(result.Failure?.Message, Is.EqualTo("DeviceKey"));
    }

    [Test]
    public async Task AuthenticateAsync_Fails_WhenHeaderDoesNotMatch()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Device-Key"] = Guid.NewGuid().ToString();

        var handler = CreateHandler();
        await InitializeHandlerAsync(handler, context);

        var result = await handler.AuthenticateAsync();

        That(result.Succeeded, Is.False);
        That(result.Failure, Is.Not.Null);
        That(result.Failure?.Message, Does.Contain("Invalid Device Key"));
    }

    private static DeviceKeyAuthenticationHandler CreateHandler()
    {
        var optionsMonitor = new TestOptionsMonitor<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions());
        return new DeviceKeyAuthenticationHandler(optionsMonitor, NullLoggerFactory.Instance, UrlEncoder.Default);
    }

    private static async Task InitializeHandlerAsync(DeviceKeyAuthenticationHandler handler, HttpContext context)
    {
        context.RequestServices ??= new ServiceCollection().BuildServiceProvider();
        var scheme = new AuthenticationScheme("DeviceKey", "DeviceKey", typeof(DeviceKeyAuthenticationHandler));
        await handler.InitializeAsync(scheme, context);
    }

    private sealed class TestOptionsMonitor<TOptions>(TOptions currentValue) : IOptionsMonitor<TOptions>
        where TOptions : class
    {
        public TOptions CurrentValue => currentValue;

        public TOptions Get(string? name) => currentValue;

        public IDisposable OnChange(Action<TOptions, string?> listener) => NullDisposable.Instance;

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
