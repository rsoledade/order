using Moq;
using Order.Domain.Entities;
using Order.Application.DTOs;
using Order.Domain.Interfaces;
using Order.Application.Events;
using Order.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Order.Application.Commands.RegisterOrder;

namespace Order.Tests.Application.Commands.RegisterOrder
{
    public class RegisterOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEventPublisher> _eventPublisherMock = new();
        private readonly Mock<ILogger<RegisterOrderCommandHandler>> _loggerMock = new();

        private RegisterOrderCommandHandler CreateHandler() =>
            new RegisterOrderCommandHandler(
                _orderRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);

        private RegisterOrderCommand CreateValidCommand()
        {
            return new RegisterOrderCommand
            {
                ExternalId = Guid.NewGuid().ToString(),
                Products = new List<ProductDto>
                {
                    new ProductDto { Name = "Test", Price = 10, Quantity = 2 }
                }
            };
        }

        [Fact]
        public async Task Handle_Should_Process_Order_Successfully()
        {
            // Arrange
            var command = CreateValidCommand();
            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order.Domain.Entities.Order)null!);
            _orderRepositoryMock.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order.Domain.Entities.Order)null!);

            // Act
            var handler = CreateHandler();
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OrderId);
            Assert.NotNull(result.Order);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderProcessedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Duplicate_When_ExternalId_Exists()
        {
            // Arrange
            var command = CreateValidCommand();
            var existingOrder = new Order.Domain.Entities.Order(command.ExternalId, new List<Product>
            {
                new Product("Test", Money.Create(10), 2)
            });
            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingOrder);

            // Act
            var handler = CreateHandler();
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Duplicate", result.Message);
            Assert.Equal(existingOrder.OrderId.Value, result.OrderId);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Mark_As_Duplicate_When_Hash_Exists()
        {
            // Arrange
            var command = CreateValidCommand();
            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order.Domain.Entities.Order)null!);
            _orderRepositoryMock.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Order.Domain.Entities.Order(command.ExternalId, new List<Product>
                {
                    new Product("Test", Money.Create(10), 2)
                }));

            // Act
            var handler = CreateHandler();
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("duplicate", result.Message, StringComparison.OrdinalIgnoreCase);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderProcessedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Rollback_And_Return_Error_On_Exception()
        {
            // Arrange
            var command = CreateValidCommand();
            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var handler = CreateHandler();
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error processing order", result.Message);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
