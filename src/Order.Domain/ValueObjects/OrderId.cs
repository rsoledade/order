namespace Order.Domain.ValueObjects
{
    public record OrderId
    {
        public Guid Value { get; private set; }

        private OrderId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("OrderId cannot be empty", nameof(value));

            Value = value;
        }

        public static OrderId Create(Guid value)
        {
            return new OrderId(value);
        }

        public static OrderId CreateNew()
        {
            return new OrderId(Guid.NewGuid());
        }

        public static implicit operator Guid(OrderId orderId) => orderId.Value;

        public override string ToString() => Value.ToString();
    }
}
