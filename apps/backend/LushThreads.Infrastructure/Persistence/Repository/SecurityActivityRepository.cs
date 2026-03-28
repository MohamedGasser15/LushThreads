using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for SecurityActivity entity.
    /// Inherits from generic Repository and implements ISecurityActivityRepository.
    /// </summary>
    public class SecurityActivityRepository : Repository<SecurityActivity>, ISecurityActivityRepository
    {
        private readonly ApplicationDbContext _db;

        public SecurityActivityRepository(ApplicationDbContext db, ILogger<Repository<SecurityActivity>> logger)
            : base(db, logger)
        {
            _db = db;
        }
    }
}
