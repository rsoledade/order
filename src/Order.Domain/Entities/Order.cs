using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public OrderId OrderId { get; private set; }
        public string ExternalId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Money TotalAmount { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? OrderHash { get; private set; }

        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        protected Order() { } // EF Core

        public Order(string externalId, IEnumerable<Product> products)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("ExternalId cannot be empty", nameof(externalId));

            if (products == null || !products.Any())
                throw new ArgumentException("Order must have at least one product", nameof(products));

            OrderId = OrderId.CreateNew();
            ExternalId = externalId;
            Status = OrderStatus.Received;
            CreatedAt = DateTime.UtcNow;
            TotalAmount = Money.Zero;

            foreach (var product in products)
            {
                AddProduct(product);
            }

            CalculateTotalAmount();
            GenerateOrderHash();
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _products.Add(product);
            CalculateTotalAmount();
        }

        public void MarkAsProcessed()
        {
            if (Status == OrderStatus.Duplicate)
                throw new InvalidOperationException("Cannot process a duplicate order");

            if (Status == OrderStatus.Error)
                throw new InvalidOperationException($"Cannot process an order with errors: {ErrorMessage}");

            Status = OrderStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkAsDuplicate()
        {
            Status = OrderStatus.Duplicate;
            ErrorMessage = "Duplicate order detected";
        }

        public void MarkAsError(string errorMessage)
        {
            Status = OrderStatus.Error;
            ErrorMessage = errorMessage ?? "Unknown error";
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = Money.Create(_products.Sum(p => p.GetTotalPrice().Value));
        }

        private void GenerateOrderHash()
        {
            // Simple hash generation for duplicate detection
            // In a real scenario, this could be more sophisticated
            var productsInfo = string.Join("|", _products.Select(p => $"{p.Name}:{p.Price.Value}:{p.Quantity}"));
            var hashInput = $"{ExternalId}|{productsInfo}";

            // Using GetHashCode is a simplification. In a real system, use a proper hashing algorithm
            OrderHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(hashInput));
        }
    }
}
