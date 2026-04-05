namespace Revorq.DAL.Entities;

public class UserBuildingAccess
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;
}
