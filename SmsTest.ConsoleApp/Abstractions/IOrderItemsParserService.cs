using SmsTest.ApiClient.Http.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ConsoleApp.Abstractions;

public interface IOrderItemsParserService
{
    bool TryParse(string? ordersString, out List<OrderItemDto> orderItems);
}
