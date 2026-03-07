using LushThreads.Infrastructure.Data;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public async Task<T> GetById(int id)
        {
            return await dbSet.FindAsync(id);
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            return await dbSet.ToListAsync();
        }
        public async Task Add(T entity)
        {
            dbSet.Add(entity);
        }
        public async Task Delete(T entity)
        {
            dbSet.Remove(entity);
        }
        public async Task AdminActivityAsync(string userId, string activityType, string description, string ipAddress)
        {
            var activity = new AdminActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                IpAddress = ipAddress
            };

            _db.AdminActivities.Add(activity);
            await _db.SaveChangesAsync();
        }
    }
}
