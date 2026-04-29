using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SmsTest.ConsoleApp.Abstractions;
using SmsTest.ConsoleApp.Extensions;
using SmsTest.ConsoleApp.Repositories;
using SmsTest.ConsoleApp.Services;
using SmsTest.Data;

namespace SmsTest.ConsoleApp;

internal class Program
{
    /// <summary>
    /// Инциализация глобального логгера 
    /// (не привязанного к хосту)
    /// </summary>
    private static void InitialiazeGlobalLogger()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    static async Task Main(string[] args)
    {
        InitialiazeGlobalLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            builder.Services.AddApiHttpClient();
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
            });

            builder.Services.AddSingleton<IOrderItemsParserService, OrderItemsParserService>();
            builder.Services.AddScoped<ISmsRepository, SmsRepository>();

            builder.Services.AddHostedService<AppWorker>();

            using var app = builder.Build();
            await app.RunAsync();
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
