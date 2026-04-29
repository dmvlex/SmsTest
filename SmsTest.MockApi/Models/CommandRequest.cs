namespace SmsTest.MockApi.Models;

public class CommandRequest
{
    public string Command { get; set; } = string.Empty;
    public CommandParameters? CommandParameters { get; set; }
}

public class CommandParameters
{
    // Для GetMenu
    public bool? WithPrice { get; set; }

    // Для SendOrder
    public string? OrderId { get; set; }
    public List<OrderItemRequest>? MenuItems { get; set; }
}

public class OrderItemRequest
{
    public string Id { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}