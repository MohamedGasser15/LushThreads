using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext db, ILogger<Repository<Brand>> logger)
            : base(db, logger)
        {
        }
    }
}
