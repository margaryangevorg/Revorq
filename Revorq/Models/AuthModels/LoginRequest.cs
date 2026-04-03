namespace Revorq.API.Models.AuthModels;

public class LoginRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
