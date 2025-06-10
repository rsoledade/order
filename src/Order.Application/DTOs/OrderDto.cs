using Order.Domain.Enums;

namespace Order.Application.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public string ExternalId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName => Status.ToString();
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
