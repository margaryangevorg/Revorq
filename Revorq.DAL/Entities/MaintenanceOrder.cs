using Revorq.DAL.Enums;

namespace Revorq.DAL.Entities;

public class MaintenanceOrder
{
    public int Id { get; set; }

    public int ElevatorId { get; set; }
    public Elevator Elevator { get; set; } = null!;

    public int? AssignedEngineerId { get; set; }
    public AppUser? AssignedEngineer { get; set; }

    public int ReporterId { get; set; }
    public AppUser Reporter { get; set; } = null!;

    public MaintenanceType MaintenanceType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;

    public string? ShortDescription { get; set; }
    public List<string> ImageUrls { get; set; } = [];

    public MaintenanceReport? Report { get; set; }
}
