namespace SmsTest.MockApi.Models;

public class GetMenuResponse
{
    public string Command { get; set; } = "GetMenu";
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public GetMenuData? Data { get; set; }
}

public class GetMenuData
{
    public List<MenuItem> MenuItems { get; set; } = new();
}

public class MenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsWeighted { get; set; }
    public string FullPath { get; set; } = string.Empty;
    public List<string> Barcodes { get; set; } = new();
}