using Order.Domain.ValueObjects;

namespace Order.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Entities.Order> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default);
        Task<Entities.Order> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task<Entities.Order> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
        Task<IEnumerable<Entities.Order>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Entities.Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Entities.Order order, CancellationToken cancellationToken = default);
    }
}
