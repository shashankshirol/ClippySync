using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ClippySync;

public class DeviceKeyAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DeviceKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : base(options, logger, encoder){}
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Device-Key", out var extractedDeviceKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("DeviceKey"));
        }
        
        var currentDeviceKey = Environment.MachineName;

        if (string.IsNullOrEmpty(currentDeviceKey) || !string.Equals(extractedDeviceKey, currentDeviceKey, StringComparison.CurrentCultureIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Device Key provided."));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "DeviceKeyUser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}