namespace Revorq.API.Models.CompanyModels;

public class CompanyResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public int MemberCount { get; set; }
    public string? LogoUrl { get; set; }
}
