using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using TextCopy;

namespace ClippySync.Web;

public abstract class ClippyWebApp
{
    public static WebApplication BuildWebApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "DeviceKey";
                options.DefaultChallengeScheme = "DeviceKey";
            })
            .AddScheme<AuthenticationSchemeOptions, DeviceKeyAuthenticationHandler>("DeviceKey", null);
        builder.Services.AddAuthorization();
        var url = builder.Configuration.GetSection("url").Value ?? "http://localhost:8877";
        builder.WebHost.UseUrls(url);

        var app = builder.Build();

        var ip = Util.GetLocalIPv4();
        var port = 8877;

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        // Endpoint to get device name
        app.MapGet("/get-device-name",
                (HttpContext httpContext) => Results.Ok(new { Message = Environment.MachineName }))
            .WithName("GetDeviceName").WithOpenApi();


        // Endpoint to get clipboard text
        app.MapGet("/clipboard", async (HttpContext httpContext) =>
        {
            var clipboardText = await ClipboardService.GetTextAsync() ?? string.Empty;
            return Results.Ok(new { Message = clipboardText });
        }).WithName("GetClipboardText").WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Device-Key",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "Device key header for authentication"
                    }
                });
                return operation;
            }
        ).RequireAuthorization();


        // Endpoint to set clipboard text
        app.MapPost("/set-clipboard", async (HttpContext httpContext) =>
            {
                using var reader = new StreamReader(httpContext.Request.Body);
                var newClipboardText = await reader.ReadToEndAsync();
                await ClipboardService.SetTextAsync(newClipboardText);
                return Results.Ok(new { Message = "Clipboard updated successfully." });
            }).WithName("SetClipboardText")
            .WithOpenApi(operation =>
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "X-Device-Key",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Description = "Device key header for authentication"
                        }
                    });
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content =
                        {
                            ["text/plain"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "The text to set in the clipboard."
                                }
                            }
                        },
                        Required = true
                    };
                    return operation;
                }
            )
            .RequireAuthorization();

        Console.WriteLine("===============================================");
        Console.WriteLine(" Clipboard Server Running ");
        Console.WriteLine("===============================================");
        Console.WriteLine($" Local IP Address:  {ip}");
        Console.WriteLine($" Listening URL:     http://{ip}:{port}");
        Console.WriteLine(" Use this in Shortcuts to connect your phone.");
        Console.WriteLine("===============================================");

        return app;
    }
}