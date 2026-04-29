using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SmsTest.ApiClient.Grpc;

namespace SmsTest.MockApi.Services;

public class SmsTestServiceImpl : SmsTestService.SmsTestServiceBase
{
    private readonly List<MenuItem> _menuItems = new()
    {
        new MenuItem
        {
            Id = "5979224",
            Article = "A1004292",
            Name = "Каша гречневая",
            Price = 50,
            IsWeighted = false,
            FullPath = "ПРОИЗВОДСТВО\\Гарниры",
            Barcodes = { "57890975627974236429" }
        },
        new MenuItem
        {
            Id = "9084246",
            Article = "A1004293",
            Name = "Конфеты Коровка",
            Price = 300,
            IsWeighted = true,
            FullPath = "ДЕСЕРТЫ\\Развес",
        },
        new MenuItem
        {
            Id = "1234567",
            Article = "B2005001",
            Name = "Борщ",
            Price = 180,
            IsWeighted = false,
            FullPath = "ПРОИЗВОДСТВО\\Супы",
            Barcodes = { "1234567890123" }
        }
    };

    public override Task<GetMenuResponse> GetMenu(BoolValue request, ServerCallContext context)
    {
        return Task.FromResult(new GetMenuResponse
        {
            Success = true,
            MenuItems = { _menuItems }
        });
    }

    public override Task<SendOrderResponse> SendOrder(Order request, ServerCallContext context)
    {
        // Всегда успех, можно добавить логику проверки заказа
        return Task.FromResult(new SendOrderResponse
        {
            Success = true
        });
    }
}
