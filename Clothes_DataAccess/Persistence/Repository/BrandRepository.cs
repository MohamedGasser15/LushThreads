using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    public class BrandRepository : Repository<Brand> , IBrandRepository
    {
        private readonly ApplicationDbContext _db;
        public BrandRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task UpdateAsync(Brand brand)
        {
            _db.Brands.Update(brand);
        }
    }
}
