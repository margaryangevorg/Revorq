using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetUserWithCompanyAsync(int id);
}
