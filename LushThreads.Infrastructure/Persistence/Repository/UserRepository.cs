using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for ApplicationUser entity.
    /// </summary>
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="db">Application database context.</param>
        /// <param name="logger">Logger instance.</param>
        public UserRepository(ApplicationDbContext db, ILogger<Repository<ApplicationUser>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _db.ApplicationUsers.CountAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.ApplicationUsers
                .Where(u => u.CreatedDate >= startDate && u.CreatedDate < endDate)
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _db.ApplicationUsers
                .Where(u => u.LockoutEnd == null || u.LockoutEnd < DateTime.UtcNow)
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetLockedUsersCountAsync()
        {
            return await _db.ApplicationUsers
                .Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow)
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task<List<ApplicationUser>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.ApplicationUsers
                .Where(u => u.CreatedDate >= startDate && u.CreatedDate < endDate)
                .ToListAsync();
        }

        #endregion
    }
}