using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for AdminActivity entity.
    /// Inherits from generic Repository and implements IAdminActivityRepository.
    /// </summary>
    public class AdminActivityRepository : Repository<AdminActivity>, IAdminActivityRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminActivityRepository(ApplicationDbContext db, ILogger<Repository<AdminActivity>> logger)
            : base(db, logger)
        {
            _db = db;
        }
    }
}