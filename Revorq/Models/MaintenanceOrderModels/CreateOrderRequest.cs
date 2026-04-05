using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateOrderRequest
{
    public int ElevatorId { get; set; }
    public int? AssignedEngineerId { get; set; }
    public MaintenanceType MaintenanceType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? ShortDescription { get; set; }
    public IFormFile? Image { get; set; }
}
