using Order.Domain.Interfaces;
using Order.Infrastructure.Data.Context;

namespace Order.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private readonly OrderDbContext _context;

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.CurrentTransaction?.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.CurrentTransaction?.RollbackAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
