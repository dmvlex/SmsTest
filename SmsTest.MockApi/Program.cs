
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using SmsTest.MockApi.Middlewares;
using SmsTest.MockApi.Models;
using SmsTest.MockApi.Services;

namespace SmsTest.MockApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null;
        });

        // Add Swagger/OpenAPI with Basic Auth support
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SMS Mock API",
                Version = "v1",
                Description = "Mock API for SMS menu and order commands with Basic Authentication"
            });

            // Configure Basic Authentication in Swagger
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Name = "Basic Auth",
                Description = "Enter your username and password",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "BasicAuth"
                }
            };

            options.AddSecurityDefinition("BasicAuth", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                { securityScheme, new List<string>() }
            };

            options.AddSecurityRequirement(securityRequirement);
        });

        var app = builder.Build();

        // Enable Swagger UI and OpenAPI endpoint
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS Mock API v1");
                options.RoutePrefix = string.Empty; // Serve at root
            });
        }

        // Helper to detect gRPC requests
        bool IsGrpcRequest(HttpContext context)
        {
            // gRPC requests have content-type "application/grpc"
            var contentType = context.Request.ContentType;
            if (contentType?.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            // Alternatively, check path prefix used by gRPC (optional)
            var path = context.Request.Path;
            if (path.HasValue && path.Value?.StartsWith("/sms.test.SmsTestService/", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        }

        // Apply BasicAuth middleware only for non-gRPC requests
        app.UseWhen(context => !IsGrpcRequest(context), appBuilder =>
        {
            appBuilder.UseMiddleware<BasicAuthMiddleware>();
        });

        // gRPC endpoint
        app.MapGrpcService<SmsTestServiceImpl>();

        // Набор демо-блюд, имитирующих ответ из тестового задания
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = "5979224",
                Article = "A1004292",
                Name = "Каша гречневая",
                Price = 50,
                IsWeighted = false,
                FullPath = "ПРОИЗВОДСТВО\\Гарниры",
                Barcodes = new List<string> { "57890975627974236429" }
            },
            new MenuItem
            {
                Id = "9084246",
                Article = "A1004293",
                Name = "Конфеты Коровка",
                Price = 300,
                IsWeighted = true,
                FullPath = "ДЕСЕРТЫ\\Развес",
                Barcodes = new List<string>()
            },
            new MenuItem
            {
                Id = "1234567",
                Article = "B2005001",
                Name = "Борщ",
                Price = 180,
                IsWeighted = false,
                FullPath = "ПРОИЗВОДСТВО\\Супы",
                Barcodes = new List<string> { "1234567890123" }
            }
        };

        app.MapPost("/ExecuteCommand", (CommandRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return Results.BadRequest("Command is required.");
            }

            switch (request.Command)
            {
                case "GetMenu":
                    var getMenuResponse = new GetMenuResponse
                    {
                        Success = true,
                        Data = new GetMenuData
                        {
                            MenuItems = menuItems
                        }
                    };
                    return Results.Ok(getMenuResponse);

                case "SendOrder":
                    // Простая имитация: всегда успех, ошибок не возвращаем
                    // Можно добавить проверку наличия Id в меню, если требуется для тестирования "негативного ответа"
                    var sendOrderResponse = new SendOrderResponse
                    {
                        Success = true
                    };
                    return Results.Ok(sendOrderResponse);

                default:
                    return Results.BadRequest(new
                    {
                        Command = request.Command,
                        Success = false,
                        ErrorMessage = $"Неизвестная команда: {request.Command}"
                    });
            }
        })
        .WithName("ExecuteCommand")
        .WithOpenApi()
        .WithSummary("Execute a command (GetMenu or SendOrder)")
        .WithDescription("Requires Basic Authentication (admin:password). Send either 'GetMenu' or 'SendOrder' command.");

        app.Run();
    }
}
