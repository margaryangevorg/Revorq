using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class InvitationToken
{
    public int Id { get; set; }
    public string Token { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Role Role { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
