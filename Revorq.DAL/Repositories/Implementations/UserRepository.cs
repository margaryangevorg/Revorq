using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetUserWithCompanyAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == id && u.Status == EntityStatus.Active);
    }

    public async Task<IEnumerable<AppUser>> GetCompanyUsersByRoleAsync(int companyId, string? roleName)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId && u.Status == EntityStatus.Active);

        if (roleName is not null)
            query = query.Where(u => _context.UserRoles
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Any(x => x.UserId == u.Id && x.Name == roleName));

        return await query.ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
