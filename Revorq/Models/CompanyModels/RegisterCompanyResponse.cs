namespace Revorq.API.Models.CompanyModels;

public class RegisterCompanyResponse
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Message { get; set; } = "Company registered successfully. Awaiting approval.";
}
