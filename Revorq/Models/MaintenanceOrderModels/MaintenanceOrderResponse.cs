using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class MaintenanceOrderResponse
{
    public int Id { get; set; }
    public int ElevatorId { get; set; }
    public string ElevatorNumberInProject { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int? AssignedEngineerId { get; set; }
    public string? AssignedEngineerName { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImagePath { get; set; }
    public MaintenanceReportResponse? Report { get; set; }
}
