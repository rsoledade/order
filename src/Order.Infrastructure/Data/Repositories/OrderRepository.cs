using Microsoft.EntityFrameworkCore;
using Order.Domain.Interfaces;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Order> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.OrderId.Value == orderId.Value, cancellationToken);
        }

        public async Task<Domain.Entities.Order> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.ExternalId == externalId, cancellationToken);
        }

        public async Task<Domain.Entities.Order> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.OrderHash == hash, cancellationToken);
        }

        public async Task<IEnumerable<Domain.Entities.Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Products)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public Task UpdateAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default)
        {
            _context.Entry(order).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}
