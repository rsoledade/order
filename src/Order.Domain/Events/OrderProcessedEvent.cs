using Order.Domain.Enums;

namespace Order.Domain.Events
{
    public class OrderProcessedEvent
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderProductEvent> Products { get; set; } = new List<OrderProductEvent>();
        public DateTime Timestamp { get; set; }
    }

    public class OrderProductEvent
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
