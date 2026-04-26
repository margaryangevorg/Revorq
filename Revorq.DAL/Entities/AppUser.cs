using Microsoft.AspNetCore.Identity;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class AppUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<MaintenanceOrder> AssignedOrders { get; set; } = [];
    public ICollection<UserBuildingAccess> BuildingAccesses { get; set; } = [];
}
