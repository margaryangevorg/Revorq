using Revorq.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Revorq.API.Validators;

public class CompanyScopedUserValidator : IUserValidator<AppUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        var errors = new List<IdentityError>();

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            errors.Add(new IdentityError
            {
                Code = "InvalidUserName",
                Description = "Username is required."
            });
            return IdentityResult.Failed(errors.ToArray());
        }

        var normalizedName = manager.NormalizeName(user.UserName);

        var duplicate = await manager.Users
            .Where(u => u.NormalizedUserName == normalizedName
                     && u.CompanyId == user.CompanyId
                     && u.Id != user.Id)
            .AnyAsync();

        if (duplicate)
            errors.Add(new IdentityError
            {
                Code = "DuplicateUserName",
                Description = $"Username '{user.UserName}' is already taken in this company."
            });

        return errors.Count == 0
            ? IdentityResult.Success
            : IdentityResult.Failed(errors.ToArray());
    }
}
