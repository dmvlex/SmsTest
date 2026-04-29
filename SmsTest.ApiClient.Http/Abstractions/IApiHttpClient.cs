using SmsTest.ApiClient.Http.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http.Abstractions;

public interface IApiHttpClient
{
    Task<IEnumerable<DishDto>> GetDishesFromMenuAsync(bool withPrice = true);
    Task SendOrderAsync(OrderDto order);
}
