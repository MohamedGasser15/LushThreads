using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Interfaces
{
    /// <summary>
    /// Interface for SecurityActivity repository.
    /// Inherits from the generic IRepository interface.
    /// </summary>
    public interface ISecurityActivityRepository : IRepository<SecurityActivity>
    {
    }
}
