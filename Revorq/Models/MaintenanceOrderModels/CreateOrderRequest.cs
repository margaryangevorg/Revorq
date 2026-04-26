using Microsoft.AspNetCore.Mvc;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.MaintenanceOrderModels;

public class CreateOrderRequest
{
    [FromForm(Name = "elevatorId")]
    public int ElevatorId { get; set; }
    [FromForm(Name = "assignedEngineerId")]
    public int? AssignedEngineerId { get; set; }
    [FromForm(Name = "maintenanceType")]
    public MaintenanceType MaintenanceType { get; set; }
    [FromForm(Name = "scheduledDate")]
    public DateTime ScheduledDate { get; set; }
    [FromForm(Name = "shortDescription")]
    public string? ShortDescription { get; set; }
    [FromForm(Name = "images")]
    public List<IFormFile> Images { get; set; } = [];
}
