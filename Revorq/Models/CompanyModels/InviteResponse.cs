using Revorq.DAL.Enums;

namespace Revorq.API.Models.CompanyModels;

public class InviteResponse
{
    public string Token { get; set; } = string.Empty;
    public Role Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}
