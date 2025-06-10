namespace Order.Application.Interfaces
{
    // This interface can be extended with additional repository methods specific to the application layer
    // It serves as a facade over the domain repository
    public interface IOrderService
    {
        Task<Domain.Entities.Order> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Domain.Entities.Order> GetOrderByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domain.Entities.Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<Domain.Entities.Order> CreateOrderAsync(string externalId, IEnumerable<Domain.Entities.Product> products, CancellationToken cancellationToken = default);
        Task ProcessOrderAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default);
    }
}
