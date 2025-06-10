using FluentAssertions;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Tests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void CreateOrder_WithValidData_ShouldCreateOrderCorrectly()
        {
            var externalId = "EXT-123";
            var products = new List<Product>
            {
                new Product("Product 1", Money.Create(100), 2),
                new Product("Product 2", Money.Create(50), 1)
            };

            var order = new Order.Domain.Entities.Order(externalId, products);

            order.ExternalId.Should().Be(externalId);
            order.Status.Should().Be(OrderStatus.Received);
            order.Products.Should().HaveCount(2);
            order.TotalAmount.Value.Should().Be(250); // (100 * 2) + (50 * 1)
            order.OrderId.Should().NotBeNull();
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void AddProduct_ShouldIncreaseProductCountAndUpdateTotalAmount()
        {
            var externalId = "EXT-123";
            var products = new List<Product>
            {
                new Product("Product 1", Money.Create(100), 1)
            };
            var order = new Order.Domain.Entities.Order(externalId, products);
            var initialTotal = order.TotalAmount.Value;

            order.AddProduct(new Product("Product 2", Money.Create(50), 2));

            order.Products.Should().HaveCount(2);
            order.TotalAmount.Value.Should().Be(initialTotal + 100); // 100 + (50 * 2)
        }

        [Fact]
        public void MarkAsProcessed_ShouldUpdateStatusAndProcessedDate()
        {
            var externalId = "EXT-123";
            var products = new List<Product>
            {
                new Product("Product 1", Money.Create(100), 1)
            };
            var order = new Order.Domain.Entities.Order(externalId, products);

            order.MarkAsProcessed();

            order.Status.Should().Be(OrderStatus.Processed);
            order.ProcessedAt.Should().NotBeNull();
            order.ProcessedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MarkAsDuplicate_ShouldUpdateStatusAndSetErrorMessage()
        {
            var externalId = "EXT-123";
            var products = new List<Product>
            {
                new Product("Product 1", Money.Create(100), 1)
            };
            var order = new Order.Domain.Entities.Order(externalId, products);

            order.MarkAsDuplicate();

            order.Status.Should().Be(OrderStatus.Duplicate);
            order.ErrorMessage.Should().Be("Duplicate order detected");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateOrder_WithInvalidExternalId_ShouldThrowArgumentException(string invalidExternalId)
        {
            var products = new List<Product>
            {
                new Product("Product 1", Money.Create(100), 1)
            };

            Action action = () => new Order.Domain.Entities.Order(invalidExternalId, products);
            action.Should().Throw<ArgumentException>().WithMessage("*ExternalId*");
        }

        [Fact]
        public void CreateOrder_WithEmptyProductList_ShouldThrowArgumentException()
        {
            var externalId = "EXT-123";
            var emptyProducts = new List<Product>();

            Action action = () => new Order.Domain.Entities.Order(externalId, emptyProducts);
            action.Should().Throw<ArgumentException>().WithMessage("*product*");
        }
    }
}
