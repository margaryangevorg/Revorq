using Revorq.DAL.Enums;

namespace Revorq.API.Models.ElevatorModels;

public class ElevatorResponse
{
    public int Id { get; set; }
    public string NumberInProject { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? ElevatorModel { get; set; }
    public string? ElevatorProductionCountry { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public WarrantyType WarrantyType { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public Priority? Priority { get; set; }
    public DateTime CreationDate { get; set; }
    public int BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
}
