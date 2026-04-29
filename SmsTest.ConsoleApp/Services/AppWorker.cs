using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmsTest.ApiClient.Http.Abstractions;
using SmsTest.ApiClient.Http.Dto;
using SmsTest.ConsoleApp.Abstractions;
using SmsTest.Data.Models;
using System.Globalization;

namespace SmsTest.ConsoleApp.Services;

public class AppWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ISmsRepository _smsRepository;
    private readonly IApiHttpClient _apiHttpClient;
    private readonly IOrderItemsParserService _orderItemsParser;
    

    public AppWorker(ILogger<AppWorker> logger,
        IApiHttpClient apiHttpClient,
        ISmsRepository smsRepository,
        IOrderItemsParserService orderItemsParser)
    {
        _logger = logger;
        _smsRepository = smsRepository;
        _apiHttpClient = apiHttpClient;
        _orderItemsParser = orderItemsParser;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Нажмите CTRL + C для выхода");

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

            _logger.LogInformation("Синхронизация списка блюд с базой данных");
            await _smsRepository.UpsertDishesAsync(mappedDishes);
            _logger.LogInformation("Список блюд синхронизирован");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка синхронизации списка блюд с базой данных");
        }

        var orderItems = new List<OrderItemDto>();
        bool isInputValid = false;

        while (!isInputValid)
        {
            var userInput = ReadOrderInput();

            if (!_orderItemsParser.TryParse(userInput, out orderItems))
            {
                _logger.LogError("Ошибка формата. Используйте: Код1:Число1;Код2:Число2;...");
                continue;
            }

            //a.kh: возможно я не правильно понял условие.
            // "...а указанное количество для всех позиций больше нуля"
            // я понял как сумма количества товара в позициях, а не количество указанных позиций
            if (orderItems.Sum(x => x.Quantity) <= 0)
            {
                _logger.LogError("Количество указанное в позициях должно быть больше 0");
                continue;
            }

            if (!await _smsRepository.IsDishesExistAsync(orderItems.Select(d=>d.Id)))
            {
                _logger.LogError("Ошибка. В заказе указано несуществующее блюдо");
                continue;
            }

            isInputValid = true;
            _logger.LogInformation($"Позиции заказа: {userInput}");
        }

        _logger.LogInformation("Отправка заказа");

        try
        {
            await _apiHttpClient.SendOrderAsync(new()
            {
                OrderId = Guid.NewGuid().ToString(),
                MenuItems = orderItems,
            });

            _logger.LogInformation("Успех!");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"Ошибка отправки заказа");
        }
    }

    private string? ReadOrderInput()
    {
        _logger.LogInformation("Введите заказ:");
        return Console.ReadLine();
    }
}
