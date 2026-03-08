using LushThreads.Domain.Entites;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for Category entity specific operations.
    /// Inherits from generic IRepository.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
    }
}