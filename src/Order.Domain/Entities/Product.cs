using Order.Domain.ValueObjects;

namespace Order.Domain.Entities
{
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Money Price { get; private set; }
        public int Quantity { get; private set; }
        public int OrderId { get; private set; }

        // Navigation property
        public Order Order { get; private set; }

        protected Product() { } // EF Core

        public Product(string name, Money price, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public Money GetTotalPrice()
        {
            return Money.Create(Price.Value * Quantity);
        }
    }
}
