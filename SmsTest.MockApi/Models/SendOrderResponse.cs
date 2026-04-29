namespace SmsTest.MockApi.Models;

public class SendOrderResponse
{
    public string Command { get; set; } = "SendOrder";
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}