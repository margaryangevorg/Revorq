using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    public byte[]? LogoData { get; set; }

    public ICollection<AppUser> Members { get; set; } = [];
    public ICollection<InvitationToken> InvitationTokens { get; set; } = [];
    public ICollection<Building> Buildings { get; set; } = [];
}
