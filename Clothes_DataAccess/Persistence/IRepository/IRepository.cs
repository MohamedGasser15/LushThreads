namespace LushThreads.Infrastructure.Persistence.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task Add(T entity);
        Task Delete(T entity);
        Task AdminActivityAsync(string userId, string activityType, string description, string ipAddress);
    }
}
