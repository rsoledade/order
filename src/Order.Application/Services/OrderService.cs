using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;
using Order.Domain.Events;
using Order.Domain.Interfaces;
using Order.Domain.ValueObjects;

namespace Order.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IEventPublisher eventPublisher,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<Domain.Entities.Order> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _orderRepository.GetByIdAsync(OrderId.Create(orderId), cancellationToken);
        }

        public async Task<Domain.Entities.Order> GetOrderByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            return await _orderRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        public async Task<IEnumerable<Domain.Entities.Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await _orderRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Domain.Entities.Order> CreateOrderAsync(string externalId, IEnumerable<Domain.Entities.Product> products, CancellationToken cancellationToken = default)
        {
            // Check for duplicate order
            var existingOrder = await _orderRepository.GetByExternalIdAsync(externalId, cancellationToken);
            if (existingOrder != null)
            {
                _logger.LogWarning("Duplicate order with ExternalId {ExternalId} detected", externalId);
                return existingOrder;
            }

            // Create a new order
            var order = new Domain.Entities.Order(externalId, products);

            // Check for duplicate by hash
            var existingOrderByHash = await _orderRepository.GetByHashAsync(order.OrderHash, cancellationToken);
            if (existingOrderByHash != null)
            {
                _logger.LogWarning("Duplicate order with hash {Hash} detected", order.OrderHash);
                order.MarkAsDuplicate();
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return order;
        }

        public async Task ProcessOrderAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default)
        {
            if (order.Status == Domain.Enums.OrderStatus.Duplicate)
            {
                _logger.LogWarning("Cannot process duplicate order with OrderId {OrderId}", order.OrderId.Value);
                return;
            }

            if (order.Status == Domain.Enums.OrderStatus.Error)
            {
                _logger.LogWarning("Cannot process order with error status: {ErrorMessage}", order.ErrorMessage);
                return;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                order.MarkAsProcessed();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event after successful processing
                await PublishOrderProcessedEvent(order, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order with OrderId {OrderId}", order.OrderId.Value);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                order.MarkAsError(ex.Message);
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task PublishOrderProcessedEvent(Domain.Entities.Order order, CancellationToken cancellationToken)
        {
            var orderEvent = new OrderProcessedEvent
            {
                OrderId = order.OrderId.Value,
                TotalAmount = order.TotalAmount.Value,
                Status = order.Status,
                Products = order.Products.Select(p => new OrderProductEvent
                {
                    Name = p.Name,
                    Price = p.Price.Value,
                    Quantity = p.Quantity
                }).ToList(),
                Timestamp = DateTime.UtcNow
            };

            await _eventPublisher.PublishAsync(orderEvent, cancellationToken);
            _logger.LogInformation("Order processed event published for OrderId: {OrderId}", order.OrderId.Value);
        }
    }
}
