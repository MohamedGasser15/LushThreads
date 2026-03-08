using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.IRepository
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
    }
}