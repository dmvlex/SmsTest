using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http.Dto;

public class OrderDto
{
    public string OrderId { get; set; }
    public List<OrderItemDto> MenuItems { get; set; }
}
