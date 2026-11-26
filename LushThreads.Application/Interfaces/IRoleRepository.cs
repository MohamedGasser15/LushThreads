using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Interfaces
{
    /// <summary>
    /// Repository interface for IdentityRole operations.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Gets all role names.
        /// </summary>
        Task<List<string>> GetAllRoleNamesAsync();

        /// <summary>
        /// Gets a mapping of userId to their primary role name in a single query.
        /// </summary>
        Task<Dictionary<string, string>> GetUserRolesMapAsync();
    }
}