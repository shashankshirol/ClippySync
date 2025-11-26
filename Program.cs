using ClippySync;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using TextCopy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "DeviceKey";
    options.DefaultChallengeScheme = "DeviceKey";
})
.AddScheme<AuthenticationSchemeOptions, DeviceKeyAuthenticationHandler>("DeviceKey", null);
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


// Endpoint to get device name
app.MapGet("/get-device-name", handler: (HttpContext httpContext) => Results.Ok(new { Message = Environment.MachineName })).WithName("GetDeviceName").WithOpenApi();


// Endpoint to get clipboard text
app.MapGet("/clipboard", handler: async (HttpContext httpContext) =>
{
    string clipboardText = await ClipboardService.GetTextAsync() ?? string.Empty;
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
            },
        });
        return operation;
    }
    ).RequireAuthorization();


// Endpoint to set clipboard text
app.MapPost(pattern: "/set-clipboard", handler: async (HttpContext httpContext) =>
{
    using var reader = new StreamReader(httpContext.Request.Body);
    string newClipboardText = await reader.ReadToEndAsync();
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

app.Run();