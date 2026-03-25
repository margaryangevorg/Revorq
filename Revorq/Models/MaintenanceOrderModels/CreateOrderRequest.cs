using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateOrderRequest
{
    public int ElevatorId { get; set; }
    public string AssignedEngineerId { get; set; } = string.Empty;
    public MaintenanceType MaintenanceType { get; set; }
    public DateTime ScheduledDate { get; set; }
}
