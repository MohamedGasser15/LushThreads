using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Data;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository implementation for OrderHeader entity.
    /// </summary>
    public class OrderRepository : Repository<OrderHeader>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db, ILogger<Repository<OrderHeader>> logger)
            : base(db, logger)
        {
            _db = db;
        }

        public async Task<OrderHeader?> GetOrderWithUserAsync(int orderId)
        {
            return await _db.OrderHeaders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersWithUserAsync()
        {
            return await _db.OrderHeaders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            return await _db.OrderDetails
                .Where(od => od.OrderHeaderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsWithProductAsync(int orderId)
        {
            return await _db.OrderDetails
                .Where(od => od.OrderHeaderId == orderId)
                .Include(od => od.Product)
                .ToListAsync();
        }

        public async Task UpdateStockForOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails, bool increase)
        {
            foreach (var detail in orderDetails)
            {
                var stock = await _db.Stocks
                    .FirstOrDefaultAsync(s => s.Product_Id == detail.ProductId && s.Size == detail.Size);

                if (stock == null)
                    throw new InvalidOperationException($"Stock not found for Product ID {detail.ProductId} with size {detail.Size}.");

                if (increase)
                    stock.Quantity += detail.Count;
                else
                {
                    if (stock.Quantity < detail.Count)
                        throw new InvalidOperationException($"Insufficient stock for Product ID {detail.ProductId} (Size: {detail.Size}). Available: {stock.Quantity}, Requested: {detail.Count}.");
                    stock.Quantity -= detail.Count;
                }

                _db.Stocks.Update(stock);
            }
        }
    }
}