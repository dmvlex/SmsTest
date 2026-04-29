using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmsTest.ApiClient.Http;
using SmsTest.ApiClient.Http.Abstractions;
using System.Text;

namespace SmsTest.ConsoleApp.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<IApiHttpClient, ApiHttpClient>((provider, httpClient) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var login = configuration.GetRequiredSection("SmsApi:Login").Value;
            var password = configuration.GetRequiredSection("SmsApi:Password").Value;
            var apiBaseUrl = configuration.GetRequiredSection("SmsApi:BaseUrl").Value;

            if (string.IsNullOrEmpty(apiBaseUrl))
                throw new ArgumentNullException("SmsApi url not specified");

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}"));

            httpClient.BaseAddress = new(apiBaseUrl);
            httpClient.DefaultRequestHeaders
                      .Authorization = new("Basic", credentials);
        });

        return services;
    }
}
