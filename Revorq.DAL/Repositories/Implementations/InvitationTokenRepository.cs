using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class InvitationTokenRepository : Repository<InvitationToken>, IInvitationTokenRepository
{
    public InvitationTokenRepository(AppDbContext context) : base(context) { }

    public async Task<InvitationToken?> GetByTokenAsync(string token)
    {
        return await _context.InvitationTokens
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Token == token);
    }
}
