using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries.GetOrders
{
    public class GetOrdersQuery : IRequest<GetOrdersResponse>
    {
        public Guid? OrderId { get; set; }
        public string? ExternalId { get; set; }
    }

    public class GetOrdersResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
