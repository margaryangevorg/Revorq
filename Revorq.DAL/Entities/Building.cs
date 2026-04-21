using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public BuildingType BuildingType { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<Elevator> Elevators { get; set; } = [];
    public ICollection<UserBuildingAccess> UserAccesses { get; set; } = [];
    public ICollection<BuildingFile> Files { get; set; } = [];
}
