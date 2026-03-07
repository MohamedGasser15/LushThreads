using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    public class ProductRepository : Repository<Product> , IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
