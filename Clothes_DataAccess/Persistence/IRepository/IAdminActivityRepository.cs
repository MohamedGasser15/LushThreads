using LushThreads.Domain.Entites;

namespace LushThreads.Infrastructure.Persistence.IRepository
{
    /// <summary>
    /// Repository interface for AdminActivity entity.
    /// Inherits generic IRepository for basic CRUD operations.
    /// </summary>
    public interface IAdminActivityRepository : IRepository<AdminActivity>
    {
    }
}