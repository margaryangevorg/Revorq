using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Models.CompanyModels;

namespace Revorq.API.Services.Interfaces;

public interface ICompanyService
{
    Task<ServiceResult<RegisterCompanyResponse>> RegisterCompanyAsync(RegisterCompanyRequest request);
    Task<ServiceResult<AuthResponse>> JoinWithTokenAsync(JoinWithTokenRequest request);
    Task<ServiceResult<InviteResponse>> GenerateInviteAsync(int companyId, InviteRequest request);
    Task<ServiceResult<bool>> ApproveCompanyAsync(int id);
    Task<ServiceResult<bool>> RejectCompanyAsync(int id);
    Task<ServiceResult<CompanyResponse>> GetCompanyAsync(int id);
    Task<ServiceResult<bool>> UpdateLogoAsync(int companyId, IFormFile logo);
}
