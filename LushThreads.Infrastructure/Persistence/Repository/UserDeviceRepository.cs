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
    /// Repository implementation for UserDevice entity.
    /// Inherits from generic Repository and implements IUserDeviceRepository.
    /// </summary>
    public class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
    {
        private readonly ApplicationDbContext _db;

        public UserDeviceRepository(ApplicationDbContext db, ILogger<Repository<UserDevice>> logger)
            : base(db, logger)
        {
            _db = db;
        }
    }
}
