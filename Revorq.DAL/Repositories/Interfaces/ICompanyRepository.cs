using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByIdWithMembersAsync(int id);
    Task<Company?> GetByNameAsync(string name);
}
