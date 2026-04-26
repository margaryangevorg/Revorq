using System.Text.Json.Serialization;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.BuildingModels;

public class BuildingRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
    [JsonPropertyName("buildingType")]
    public BuildingType BuildingType { get; set; }
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }
    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
    [JsonPropertyName("files")]
    public List<IFormFile> Files { get; set; } = [];
}
