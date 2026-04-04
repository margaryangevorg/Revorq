using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public BuildingType BuildingType { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<Elevator> Elevators { get; set; } = [];
}
