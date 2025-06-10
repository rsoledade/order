using MediatR;
using Order.Domain.Entities;
using Order.Application.DTOs;
using Order.Domain.Interfaces;
using Order.Application.Events;
using Order.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Order.Application.Commands.RegisterOrder
{
    public class RegisterOrderCommandHandler : IRequestHandler<RegisterOrderCommand, RegisterOrderResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RegisterOrderCommandHandler> _logger;

        public RegisterOrderCommandHandler(IOrderRepository orderRepository,
            IUnitOfWork unitOfWork, IEventPublisher eventPublisher, ILogger<RegisterOrderCommandHandler> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
            _orderRepository = orderRepository;
        }

        public async Task<RegisterOrderResponse> Handle(RegisterOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing order with ExternalId: {ExternalId}", request.ExternalId);

                // Check for duplicate by ExternalId
                var existingOrderByExternalId = await _orderRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);
                if (existingOrderByExternalId != null)
                {
                    _logger.LogWarning("Duplicate order detected by ExternalId: {ExternalId}", request.ExternalId);
                    return new RegisterOrderResponse
                    {
                        Success = false,
                        Message = "Duplicate order detected by ExternalId",
                        OrderId = existingOrderByExternalId.OrderId.Value
                    };
                }

                // Create products from DTOs
                var products = request.Products.Select(p => new Product(
                    p.Name,
                    Money.Create(p.Price),
                    p.Quantity
                )).ToList();

                // Create order
                var order = new Domain.Entities.Order(request.ExternalId, products);

                // Check for duplicate by hash
                var existingOrderByHash = await _orderRepository.GetByHashAsync(order.OrderHash, cancellationToken);
                if (existingOrderByHash != null)
                {
                    _logger.LogWarning("Duplicate order detected by hash: {Hash}", order.OrderHash);
                    order.MarkAsDuplicate();
                }

                // Start transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Persist order
                await _orderRepository.AddAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Process the order if not duplicate
                if (order.Status != Domain.Enums.OrderStatus.Duplicate)
                {
                    order.MarkAsProcessed();
                    await _orderRepository.UpdateAsync(order, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Publish event after successful processing
                    await PublishOrderProcessedEvent(order, cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Order processed successfully. OrderId: {OrderId}, Status: {Status}",
                    order.OrderId.Value, order.Status);

                return new RegisterOrderResponse
                {
                    Success = true,
                    Message = $"Order {(order.Status == Domain.Enums.OrderStatus.Processed ? "processed" : "marked as duplicate")} successfully",
                    OrderId = order.OrderId.Value,
                    Order = MapToOrderDto(order)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order with ExternalId: {ExternalId}", request.ExternalId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                return new RegisterOrderResponse
                {
                    Success = false,
                    Message = $"Error processing order: {ex.Message}"
                };
            }
        }

        private async Task PublishOrderProcessedEvent(Domain.Entities.Order order, CancellationToken cancellationToken)
        {
            var orderEvent = new OrderProcessedEvent(
                order.OrderId.Value,
                order.TotalAmount.Value,
                order.Status,
                order.Products.Select(p => new OrderProductEvent
                {
                    Name = p.Name,
                    Price = p.Price.Value,
                    Quantity = p.Quantity
                }).ToList()
            );

            await _eventPublisher.PublishAsync(orderEvent, cancellationToken);
            _logger.LogInformation("Order processed event published for OrderId: {OrderId}", order.OrderId.Value);
        }

        private OrderDto MapToOrderDto(Domain.Entities.Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId.Value,
                ExternalId = order.ExternalId,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ProcessedAt = order.ProcessedAt,
                TotalAmount = order.TotalAmount.Value,
                Products = order.Products.Select(p => new ProductDto
                {
                    Name = p.Name,
                    Price = p.Price.Value,
                    Quantity = p.Quantity
                }).ToList()
            };
        }
    }
}
