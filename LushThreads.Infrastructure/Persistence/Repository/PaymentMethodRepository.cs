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
    /// Repository implementation for PaymentMethod entity.
    /// Inherits from generic Repository and implements IPaymentMethodRepository.
    /// </summary>
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentMethodRepository(ApplicationDbContext db, ILogger<Repository<PaymentMethod>> logger)
            : base(db, logger)
        {
            _db = db;
        }
    }
}
