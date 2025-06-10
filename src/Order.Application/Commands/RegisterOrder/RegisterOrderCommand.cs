using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands.RegisterOrder
{
    public class RegisterOrderCommand : IRequest<RegisterOrderResponse>
    {
        public string ExternalId { get; set; }
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }

    public class RegisterOrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? OrderId { get; set; }
        public OrderDto Order { get; set; }
    }
}
