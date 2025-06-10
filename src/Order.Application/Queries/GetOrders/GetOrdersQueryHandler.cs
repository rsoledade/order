using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Domain.Interfaces;
using Order.Domain.ValueObjects;

namespace Order.Application.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetOrdersQueryHandler> _logger;

        public GetOrdersQueryHandler(IOrderRepository orderRepository, ILogger<GetOrdersQueryHandler> logger)
        {
            _logger = logger;
            _orderRepository = orderRepository;
        }

        public async Task<GetOrdersResponse> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting orders with filters: OrderId={OrderId}, ExternalId={ExternalId}", request.OrderId, request.ExternalId);

                // Retrieve orders based on filters
                if (request.OrderId.HasValue)
                {
                    var orderId = OrderId.Create(request.OrderId.Value);
                    var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

                    if (order == null)
                    {
                        return new GetOrdersResponse
                        {
                            Success = true,
                            Message = "No orders found",
                            Orders = new List<OrderDto>()
                        };
                    }

                    return new GetOrdersResponse
                    {
                        Success = true,
                        Message = "Order retrieved successfully",
                        Orders = new List<OrderDto> { MapToOrderDto(order) }
                    };
                }
                else if (!string.IsNullOrEmpty(request.ExternalId))
                {
                    var order = await _orderRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

                    if (order == null)
                    {
                        return new GetOrdersResponse
                        {
                            Success = true,
                            Message = "No orders found",
                            Orders = new List<OrderDto>()
                        };
                    }

                    return new GetOrdersResponse
                    {
                        Success = true,
                        Message = "Order retrieved successfully",
                        Orders = new List<OrderDto> { MapToOrderDto(order) }
                    };
                }
                else
                {
                    // Get all orders
                    var orders = await _orderRepository.GetAllAsync(cancellationToken);
                    var orderDtos = orders.Select(MapToOrderDto).ToList();

                    return new GetOrdersResponse
                    {
                        Success = true,
                        Message = $"Retrieved {orderDtos.Count} orders",
                        Orders = orderDtos
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");

                return new GetOrdersResponse
                {
                    Success = false,
                    Message = $"Error retrieving orders: {ex.Message}",
                    Orders = new List<OrderDto>()
                };
            }
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
