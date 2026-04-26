using Microsoft.AspNetCore.Mvc;
using Revorq.DAL.Enums;

namespace Revorq.API.Models.BuildingModels;

public class BuildingRequest
{
    [FromForm(Name = "name")]
    public string Name { get; set; } = string.Empty;
    [FromForm(Name = "address")]
    public string Address { get; set; } = string.Empty;
    [FromForm(Name = "buildingType")]
    public BuildingType BuildingType { get; set; }
    [FromForm(Name = "latitude")]
    public double? Latitude { get; set; }
    [FromForm(Name = "longitude")]
    public double? Longitude { get; set; }
    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; set; } = [];
}
