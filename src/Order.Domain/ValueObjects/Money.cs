namespace Order.Domain.ValueObjects
{
    public record Money
    {
        public decimal Value { get; private set; }

        private Money(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Money value cannot be negative", nameof(value));

            Value = value;
        }

        public static Money Create(decimal value)
        {
            return new Money(value);
        }

        public static Money Zero => new Money(0);

        public static implicit operator decimal(Money money) => money.Value;

        public static Money operator +(Money a, Money b) =>
            new Money(a.Value + b.Value);

        public static Money operator -(Money a, Money b)
        {
            if (a.Value < b.Value)
                throw new InvalidOperationException("Cannot subtract to a negative value");

            return new Money(a.Value - b.Value);
        }

        public override string ToString() => Value.ToString("C");
    }
}
