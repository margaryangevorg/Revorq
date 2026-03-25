using Revorq.DAL.Enums;

namespace Revorq.API.Models.CompanyModels;

public class InviteRequest
{
    public Role Role { get; set; }
    public int ExpiryHours { get; set; } = 48;
}
