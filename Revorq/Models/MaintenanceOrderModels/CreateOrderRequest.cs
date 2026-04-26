using System.Text.Json.Serialization;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateOrderRequest
{
    [JsonPropertyName("elevatorId")]
    public int ElevatorId { get; set; }
    [JsonPropertyName("assignedEngineerId")]
    public int? AssignedEngineerId { get; set; }
    [JsonPropertyName("maintenanceType")]
    public MaintenanceType MaintenanceType { get; set; }
    [JsonPropertyName("scheduledDate")]
    public DateTime ScheduledDate { get; set; }
    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }
    [JsonPropertyName("images")]
    public List<IFormFile> Images { get; set; } = [];
}
