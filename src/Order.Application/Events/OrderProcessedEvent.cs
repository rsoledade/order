using Order.Domain.Enums;

namespace Order.Application.Events
{
    public class OrderProcessedEvent
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderProductEvent> Products { get; set; }
        public DateTime Timestamp { get; set; }

        public OrderProcessedEvent(
            Guid orderId,
            decimal totalAmount,
            OrderStatus status,
            List<OrderProductEvent> products)
        {
            OrderId = orderId;
            TotalAmount = totalAmount;
            Status = status;
            Products = products;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class OrderProductEvent
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
