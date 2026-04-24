using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class UpdateOrderRequest
{
    public MaintenanceType MaintenanceType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? ShortDescription { get; set; }
}
