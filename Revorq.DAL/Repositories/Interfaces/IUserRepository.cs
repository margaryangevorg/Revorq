using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetUserWithCompanyAsync(int id);
    Task<IEnumerable<AppUser>> GetCompanyUsersByRoleAsync(int companyId, string? roleName);
    Task<AppUser?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}
