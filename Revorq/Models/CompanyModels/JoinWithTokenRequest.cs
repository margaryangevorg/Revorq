namespace Revorq.API.Models.CompanyModels;

public class JoinWithTokenRequest
{
    public string InviteToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
