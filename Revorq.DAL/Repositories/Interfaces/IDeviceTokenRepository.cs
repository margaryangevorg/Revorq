using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IDeviceTokenRepository : IRepository<DeviceToken>
{
    Task<DeviceToken?> GetByTokenAsync(string token);
    Task<IEnumerable<DeviceToken>> GetByUserIdsAsync(IEnumerable<int> userIds);
    Task DeleteByTokenAsync(string token);
}
