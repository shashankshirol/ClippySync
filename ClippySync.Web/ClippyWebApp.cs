using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi;
using System.Text.Json;
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
        var port = Util.GetPort();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        // Endpoint to get device name
        app.MapGet("/get-device-name",
                (HttpContext httpContext) => Results.Ok(new { Message = Environment.MachineName }))
            .WithName("GetDeviceName");


        // Endpoint to get clipboard text
        app.MapGet("/clipboard", async (HttpContext httpContext) =>
        {
            var clipboardText = await ClipboardService.GetTextAsync() ?? string.Empty;
            return Results.Ok(new { Message = clipboardText });
        }).WithName("GetClipboardText").AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Parameters!.Add(new OpenApiParameter
                {
                    Name = "X-Device-Key",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Description = "Device key header for authentication"
                    }
                });
                return Task.CompletedTask;
            }
        ).RequireAuthorization();


        // Endpoint to set clipboard text
        app.MapPost("/set-clipboard", static async (HttpContext httpContext) =>
            {
                var body = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(httpContext.Request.Body);
                var newClipboardText = body!["clipboard"];
                await ClipboardService.SetTextAsync(newClipboardText);
                return Results.Ok(new { Message = "Clipboard updated successfully." });
            }).WithName("SetClipboardText")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Parameters!.Add(new OpenApiParameter
                    {
                        Name = "X-Device-Key",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Description = "Device key header for authentication"
                        }
                    });
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Object,
                                    Description = "The text to set in the clipboard."
                                }
                            }
                        },
                        Required = true
                    };
                    return Task.CompletedTask;
                }
            )
            .RequireAuthorization();

        return app;
    }
}