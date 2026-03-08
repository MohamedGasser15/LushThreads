using LushThreads.Domain.Entites;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for UserDevice entity.
    /// </summary>
    public interface IUserDeviceRepository : IRepository<UserDevice>
    {
        /// <summary>
        /// Gets a user device by user ID and device token.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="deviceToken">The device token.</param>
        /// <returns>The user device if found, otherwise null.</returns>
        Task<UserDevice?> GetByUserAndTokenAsync(string userId, string deviceToken);
    }
}