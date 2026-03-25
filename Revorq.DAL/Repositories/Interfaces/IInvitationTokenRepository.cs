using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IInvitationTokenRepository : IRepository<InvitationToken>
{
    Task<InvitationToken?> GetByTokenAsync(string token);
}
