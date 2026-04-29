using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmsTest.ApiClient.Http.Abstractions;
using SmsTest.ConsoleApp.Abstractions;
using SmsTest.Data.Models;

namespace SmsTest.ConsoleApp.Services;

public class AppWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ISmsRepository _smsRepository;
    private readonly IApiHttpClient _apiHttpClient;
    private readonly IHostApplicationLifetime _hostLifetime;
    

    public AppWorker(ILogger<AppWorker> logger,
        IApiHttpClient apiHttpClient,
        ISmsRepository smsRepository,
        IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _smsRepository = smsRepository;
        _hostLifetime = hostLifetime;
        _apiHttpClient = apiHttpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //a.kh: Накатываю последнюю миграцию, если она не была применена
        await _smsRepository.EnsureMigrateDatabaseAsync();

        try
        {
            var dishesFromServer = await _apiHttpClient.GetDishesFromMenuAsync();
            foreach (var dish in dishesFromServer)
                _logger.LogInformation(dish.ToString());

            //a.kh: в реальном проекте я бы использовал стороннюю библиотеку для маппинга
            //ну или вынес его в метод отдельный, но в данном случае достаточно ручного маппинга
            var mappedDishes = dishesFromServer.Select(d => new Dish()
            {
                Id = d.Id,
                Name = d.Name,
                Article = d.Article,
                Barcodes = d.Barcodes,
                FullPath = d.FullPath,
                IsWeighted = d.IsWeighted,
                Price = d.Price,
            });

            await _smsRepository.UpsertDishesAsync(mappedDishes);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка синхронизации списка блюд с базой данных");
            _hostLifetime.StopApplication();
        }


    }
}
