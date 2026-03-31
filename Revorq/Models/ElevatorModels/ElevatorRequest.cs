using Revorq.DAL.Enums;

namespace Revorq.API.Models.ElevatorModels;

public class ElevatorRequest
{
    public string NumberInProject { get; set; }
    public required string SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? ProductionCountry { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public WarrantyType WarrantyType { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public int BuildingId { get; set; }
}
