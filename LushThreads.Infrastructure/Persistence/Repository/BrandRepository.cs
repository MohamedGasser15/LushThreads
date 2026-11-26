using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Application.Interfaces;
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
