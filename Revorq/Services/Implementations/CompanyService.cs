using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Models.CompanyModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IInvitationTokenRepository _tokenRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;

    public CompanyService(
        ICompanyRepository companyRepository,
        IInvitationTokenRepository tokenRepository,
        UserManager<AppUser> userManager,
        IJwtService jwtService)
    {
        _companyRepository = companyRepository;
        _tokenRepository = tokenRepository;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<ServiceResult<RegisterCompanyResponse>> RegisterCompanyAsync(RegisterCompanyRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) is not null)
            return ServiceResult<RegisterCompanyResponse>.Error("Username is already taken.");

        var company = new Company
        {
            Name = request.CompanyName,
            Status = CompanyStatus.Pending,
            RegisteredAt = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company);
        await _companyRepository.SaveChangesAsync();

        var (firstName, lastName) = SplitFullName(request.FullName);

        var user = new AppUser
        {
            UserName = request.Username,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = request.Phone,
            CompanyId = company.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _companyRepository.Delete(company);
            await _companyRepository.SaveChangesAsync();
            return ServiceResult<RegisterCompanyResponse>.Error(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, Role.Admin.ToString());

        return ServiceResult<RegisterCompanyResponse>.Ok(new RegisterCompanyResponse
        {
            CompanyId = company.Id,
            CompanyName = company.Name,
            AdminUsername = user.UserName!,
            FullName = $"{user.FirstName} {user.LastName}"
        });
    }

    public async Task<ServiceResult<AuthResponse>> JoinWithTokenAsync(JoinWithTokenRequest request)
    {
        var invite = await _tokenRepository.GetByTokenAsync(request.InviteToken);

        if (invite is null)
            return ServiceResult<AuthResponse>.Error("Invalid invite token.");

        if (invite.IsUsed)
            return ServiceResult<AuthResponse>.Error("This invite token has already been used.");

        if (invite.ExpiresAt < DateTime.Now)
            return ServiceResult<AuthResponse>.Error("This invite token has expired.");

        if (invite.Company.Status != CompanyStatus.Approved)
            return ServiceResult<AuthResponse>.Error("This company is not active.");

        if (await _userManager.FindByNameAsync(request.Username) is not null)
            return ServiceResult<AuthResponse>.Error("Username is already taken.");

        var (firstName, lastName) = SplitFullName(request.FullName);

        var user = new AppUser
        {
            UserName = request.Username,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = request.Phone,
            CompanyId = invite.CompanyId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ServiceResult<AuthResponse>.Error(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, invite.Role.ToString());

        invite.IsUsed = true;
        _tokenRepository.Update(invite);
        await _tokenRepository.SaveChangesAsync();

        var token = await _jwtService.GenerateTokenAsync(user);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = invite.Role.ToString(),
            ExpiresAt = DateTime.Now.AddDays(7)
        });
    }

    public async Task<ServiceResult<InviteResponse>> GenerateInviteAsync(int companyId, InviteRequest request)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company is null)
            return ServiceResult<InviteResponse>.NotFound($"Company {companyId} not found.");

        if (company.Status != CompanyStatus.Approved)
            return ServiceResult<InviteResponse>.Error("Cannot generate invites for a non-approved company.");

        var expiresAt = DateTime.Now.AddHours(request.ExpiryHours);

        var invite = new InvitationToken
        {
            Token = Guid.NewGuid().ToString("N"),
            CompanyId = companyId,
            Role = request.Role,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.Now
        };

        await _tokenRepository.AddAsync(invite);
        await _tokenRepository.SaveChangesAsync();

        return ServiceResult<InviteResponse>.Ok(new InviteResponse
        {
            Token = invite.Token,
            Role = invite.Role,
            ExpiresAt = expiresAt
        });
    }

    public async Task<ServiceResult<bool>> ApproveCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company is null)
            return ServiceResult<bool>.NotFound($"Company {id} not found.");

        company.Status = CompanyStatus.Approved;
        _companyRepository.Update(company);
        await _companyRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RejectCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company is null)
            return ServiceResult<bool>.NotFound($"Company {id} not found.");

        company.Status = CompanyStatus.Rejected;
        _companyRepository.Update(company);
        await _companyRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<CompanyResponse>> GetCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdWithMembersAsync(id);
        if (company is null)
            return ServiceResult<CompanyResponse>.NotFound($"Company {id} not found.");

        return ServiceResult<CompanyResponse>.Ok(new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            Status = company.Status.ToString(),
            RegisteredAt = company.RegisteredAt,
            MemberCount = company.Members.Count
        });
    }

    private static (string firstName, string lastName) SplitFullName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], string.Empty);
    }
}
