using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using SmsTest.ApiClient.Grpc.Abstractions;


namespace SmsTest.ApiClient.Grpc;

/// <summary>
/// Grpc клиент для внешнего api
/// </summary>
public class ApiGrpcClient : IApiGrpcClient
{
    private SmsTestService.SmsTestServiceClient _grpcClient;

    public ApiGrpcClient(string apiGrpcUrl)
    {
        var channel = GrpcChannel.ForAddress(apiGrpcUrl);
        _grpcClient = new SmsTestService.SmsTestServiceClient(channel);
    }

    /// <summary>
    /// Получает список блюд из меню. (Команда "GetMenu")
    /// </summary>
    /// <param name="withPrice">Получить только блюда с указанной ценой</param>
    /// <remarks>a.kh: Насчет назначения параметра я был не уверен, потому мог ошибиться с его сутью</remarks>
    /// <returns>Список блюд в меню</returns>
    public async Task<IEnumerable<MenuItem>> GetDishesFromMenuAsync(bool withPrice = true)
    {
        var getMenuResult = await _grpcClient.GetMenuAsync(request: new() { Value = withPrice });

        if (!getMenuResult.Success)
            throw new HttpRequestException($"Command: GetMenu. Error Message:{getMenuResult.ErrorMessage}");

        //a.kh: без проверки на null, потому что MenuItems не nullable
        return getMenuResult.MenuItems;
    }

    /// <summary>
    /// Отправка заказа на сервер. (Команда "SendOrder")
    /// </summary>
    public async Task SendOrderAsync(Order order)
    {
        var sendOrderResult = await _grpcClient.SendOrderAsync(order);

        if (!sendOrderResult.Success)
            throw new HttpRequestException($"Command: GetMenu. Error Message:{sendOrderResult.ErrorMessage}");
    }
}
