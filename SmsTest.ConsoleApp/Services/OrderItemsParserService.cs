using Microsoft.Extensions.Logging;
using SmsTest.ApiClient.Http.Dto;
using SmsTest.ConsoleApp.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ConsoleApp.Services;

public class OrderItemsParserService : IOrderItemsParserService
{
    /// <summary>
    /// Парсит строку формата Код1:Количество1;Код2:Количество2; в список позиций заказа
    /// </summary>
    /// <param name="ordersString">Исходная строка с позициями</param>
    /// <param name="orderItems">Список позиций заказов</param>
    /// <returns>True - если формат строки верный, иначе - false</returns>
    public bool TryParse(string? ordersString, out List<OrderItemDto> orderItems)
    {
        orderItems = new();
        var temp = new List<OrderItemDto>();

        if (string.IsNullOrWhiteSpace(ordersString))
            return false;

        var entries = ordersString.Split(';',StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in entries)
        {
            //part[0] - id, a part[1] - quatity
            var parts = item.TrimEnd(';').Split(':');

            if ((parts.Length < 2) || (parts.Any(p=>string.IsNullOrWhiteSpace(p))))
                return false;

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture,out double quantity))
                return false;

            temp.Add(new()
            {
                Id = parts[0],
                Quantity = quantity
            });
        }

        orderItems = temp;
        return true;
    }
}
