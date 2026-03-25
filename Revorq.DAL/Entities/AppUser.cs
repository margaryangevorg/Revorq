using Microsoft.AspNetCore.Identity;

namespace Revorq.DAL.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public ICollection<MaintenanceOrder> AssignedOrders { get; set; } = [];
}
