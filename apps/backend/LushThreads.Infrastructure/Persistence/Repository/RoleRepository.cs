using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for IdentityRole.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        #region Fields

        private readonly ApplicationDbContext _db;
        private readonly ILogger<RoleRepository> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRepository"/> class.
        /// </summary>
        /// <param name="db">Application database context.</param>
        /// <param name="logger">Logger instance.</param>
        public RoleRepository(ApplicationDbContext db, ILogger<RoleRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<List<string>> GetAllRoleNamesAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all role names.");
                var roles = await _db.Roles.Select(r => r.Name).ToListAsync();
                _logger.LogInformation("Retrieved {Count} role names.", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving role names.");
                throw;
            }
        }

        #endregion
    }
}