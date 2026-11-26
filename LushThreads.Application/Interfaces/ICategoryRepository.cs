using LushThreads.Domain.Entites;

namespace LushThreads.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Category entity specific operations.
    /// Inherits from generic IRepository.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
    }
}