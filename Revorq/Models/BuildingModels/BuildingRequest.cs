using Revorq.DAL.Enums;

namespace Revorq.API.Models.BuildingModels;

public class BuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public BuildingType BuildingType { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
