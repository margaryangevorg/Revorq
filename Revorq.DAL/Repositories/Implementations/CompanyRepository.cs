using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context) { }

    public async Task<Company?> GetByIdWithMembersAsync(int id)
    {
        return await _context.Companies
            .Include(c => c.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
