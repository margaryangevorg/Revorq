using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class MaintenanceOrderResponse
{
    public int Id { get; set; }
    public int ElevatorId { get; set; }
    public string ElevatorNumberInProject { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string BuildingAddress { get; set; } = string.Empty;
    public double? BuildingLatitude { get; set; }
    public double? BuildingLongitude { get; set; }
    public int? AssignedEngineerId { get; set; }
    public string? AssignedEngineerName { get; set; }
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShortDescription { get; set; }
    public MaintenanceReportResponse? Report { get; set; }
}
