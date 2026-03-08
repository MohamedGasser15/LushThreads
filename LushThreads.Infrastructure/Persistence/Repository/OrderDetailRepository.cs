using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for OrderDetail entity.
    /// Inherits from generic Repository and implements IOrderDetailRepository.
    /// </summary>
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderDetailRepository(ApplicationDbContext db, ILogger<Repository<OrderDetail>> logger)
            : base(db, logger)
        {
            _db = db;
        }
    }
}
