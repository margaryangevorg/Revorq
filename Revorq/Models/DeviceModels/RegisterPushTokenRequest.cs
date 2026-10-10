namespace Revorq.API.Models.DeviceModels;

public class RegisterPushTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
