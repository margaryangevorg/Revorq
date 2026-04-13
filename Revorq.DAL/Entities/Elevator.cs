using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class Elevator
{
    public int Id { get; set; }
    public string NumberInProject { get; set; }
    public required string SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? ProductionCountry { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public WarrantyType WarrantyType { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public Priority? Priority { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public ICollection<MaintenanceOrder> MaintenanceOrders { get; set; } = [];
}
