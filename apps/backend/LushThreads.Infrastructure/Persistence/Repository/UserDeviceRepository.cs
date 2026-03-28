using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for UserDevice entity.
    /// </summary>
    public class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        public UserDeviceRepository(ApplicationDbContext db, ILogger<Repository<UserDevice>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<UserDevice?> GetByUserAndTokenAsync(string userId, string deviceToken)
        {
            return await _db.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == deviceToken);
        }

        #endregion
    }
}