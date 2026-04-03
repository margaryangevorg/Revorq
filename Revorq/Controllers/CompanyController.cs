using Revorq.API.Models.CompanyModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/company")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public CompanyController(ICompanyService companyService, IUserService userService, IConfiguration configuration)
    {
        _companyService = companyService;
        _userService = userService;
        _configuration = configuration;
    }

    // Called by mobile app — no auth required
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request)
    {
        var result = await _companyService.RegisterCompanyAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    // Called by mobile app — no auth required
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinWithTokenRequest request)
    {
        var result = await _companyService.JoinWithTokenAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    // Admin generates invite token for a new team member
    [HttpPost("invite")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GenerateInvite([FromBody] InviteRequest request)
    {
        var companyId = GetCompanyId();
        if (companyId is null)
            return BadRequest("Company not found in token.");

        var result = await _companyService.GenerateInviteAsync(companyId.Value, request);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<IActionResult> GetCompanyUsers(Role? role)
    {
        var companyId = GetCompanyId();
        if (companyId is null)
            return BadRequest("Company not found in token.");

        var result = await _userService.GetCompanyUsersAsync(companyId.Value, role);
        return Ok(result.Data);
    }

    // Admin views their own company
    [HttpGet("me")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GetMyCompany()
    {
        var companyId = GetCompanyId();
        if (companyId is null)
            return BadRequest("Company not found in token.");

        var result = await _companyService.GetCompanyAsync(companyId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    // Called by the external platform application — protected by API key
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!IsValidPlatformKey())
            return Unauthorized("Invalid platform key.");

        var result = await _companyService.ApproveCompanyAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);

        return Ok(new { message = "Company approved." });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        if (!IsValidPlatformKey())
            return Unauthorized("Invalid platform key.");

        var result = await _companyService.RejectCompanyAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);

        return Ok(new { message = "Company rejected." });
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirstValue("CompanyId");
        return int.TryParse(claim, out var id) ? id : null;
    }

    private bool IsValidPlatformKey()
    {
        var expectedKey = _configuration["Platform:ApiKey"];
        Request.Headers.TryGetValue("X-Platform-Key", out var providedKey);
        return !string.IsNullOrEmpty(expectedKey) && expectedKey == providedKey;
    }
}
