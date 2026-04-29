using SmsTest.ApiClient.Http.Abstractions;
using SmsTest.ApiClient.Http.Dto;
using SmsTest.ApiClient.Http.Dto.Responses;
using SmsTest.ApiClient.Http.Dto.Responses.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http;

/// <summary>
/// Http клиент для внешнего api
/// </summary>
public class ApiHttpClient : IApiHttpClient
{
    private readonly HttpClient _httpClient;

    //a.kh: в тз не обозначено название эндпоинта, потому я его назвал /executecommand
    public ApiHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Сериализует тело запроса для /executecommand
    /// </summary>
    /// <param name="command">Имя выполняемой команды для api</param>
    /// <param name="commandParameters">Параметры команды</param>
    /// <returns>Контент для HTTP-запроса</returns>
    private StringContent SerializeApiRequestBody(string command, object commandParameters)
    {
        var requestBody = new
        {
            Command = command,
            CommandParameters = commandParameters
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        return requestContent;
    }

    /// <summary>
    /// Вызывает выполнение команды через запрос к /executecommand
    /// </summary>
    /// <typeparam name="TResponse">Модель ответа</typeparam>
    /// <param name="command">Имя выполняемой команды для api</param>
    /// <param name="commandParameters">Параметры команды</param>
    /// <returns>Сериализованный ответ от api</returns>
    private async Task<TResponse> CallExecuteCommandAsync<TResponse>(string command, object commandParameters)
        where TResponse : ApiResposeDto
    {
        var requestContent = SerializeApiRequestBody(command, commandParameters);
        var response = await _httpClient.PostAsync("executecommand", requestContent);

        //a.kh: В ТЗ сказано, что при любом запросе будет 200.
        //Но я решил не исключать вариант выпадения 500-тки при проблемах с сервером.
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Command: {command}. Unexpected HTTP status: {response.StatusCode}");

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TResponse>(responseJson);

        //a.kh: По условию из ТЗ - Api не может вернуть полностью пустой ответ,
        //потому будем считать такой рассклад за ошибку.
        if (result == null)
            throw new InvalidOperationException(
                $"Command: {command}. Result deserialization returned null.");

        if (!result.Success)
            throw new HttpRequestException($"Command: {command}. Error Message:{result.ErrorMesage}");

        return result;
    }

    /// <summary>
    /// Получает список блюд из меню. (Команда "GetMenu")
    /// </summary>
    /// <param name="withPrice">Получить только блюда с указанной ценой</param>
    /// <remarks>a.kh: Насчет назначения параметра я был не уверен, потому мог ошибиться с его сутью</remarks>
    /// <returns>Список блюд в меню</returns>
    public async Task<IEnumerable<DishDto>> GetDishesFromMenuAsync(bool withPrice = true)
    {
        var getMenuResult = await CallExecuteCommandAsync<ApiResposeDto<GetMenuDataDto>>("GetMenu", new
        {
            WithPrice = withPrice
        });

        //a.kh: в тз не был уточнен этот момент. 
        //Но я думаю, что если у нас Success = true и Data = null, то
        //можно считать, что необходимое меню не было найдено и значит нужных блюд нет
        return getMenuResult.Data?.MenuItems ?? Enumerable.Empty<DishDto>();
    }

    /// <summary>
    /// Отправка заказа на сервер. (Команда "SendOrder")
    /// </summary>
    /// <remarks>a.kh: здесь я пробросил задачу, что бы если мы отправляем несколько 
    /// заказов их можно было вызвать через Task.WhenAll(), например.</remarks>
    public Task SendOrderAsync(OrderDto order)
        => CallExecuteCommandAsync<ApiResposeDto>("SendOrder", order);

}
