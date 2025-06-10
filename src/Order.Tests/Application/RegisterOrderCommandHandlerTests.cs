using Moq;
using FluentAssertions;
using Order.Domain.Entities;
using Order.Application.DTOs;
using Order.Domain.Interfaces;
using Order.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Order.Application.Commands.RegisterOrder;

namespace Order.Tests.Application
{
    public class RegisterOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ILogger<RegisterOrderCommandHandler>> _loggerMock;
        private readonly RegisterOrderCommandHandler _handler;

        public RegisterOrderCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<RegisterOrderCommandHandler>>();

            _handler = new RegisterOrderCommandHandler(
                _orderRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateAndProcessOrder()
        {
            // Arrange
            var command = new RegisterOrderCommand
            {
                ExternalId = "EXT-123",
                Products = new List<ProductDto>
                {
                    new ProductDto { Name = "Product 1", Price = 100, Quantity = 2 },
                    new ProductDto { Name = "Product 2", Price = 50, Quantity = 1 }
                }
            };

            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order.Domain.Entities.Order)null);

            _orderRepositoryMock.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order.Domain.Entities.Order)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.OrderId.Should().NotBeNull();
            result.Message.Should().Contain("processed successfully");

            _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDuplicateExternalId_ShouldReturnFailureResponse()
        {
            // Arrange
            var command = new RegisterOrderCommand
            {
                ExternalId = "EXT-123",
                Products = new List<ProductDto>
                {
                    new ProductDto { Name = "Product 1", Price = 100, Quantity = 2 }
                }
            };

            var existingOrder = new Order.Domain.Entities.Order("EXT-123", new List<Product>
            {
                new Product("Product 1", Money.Create(100), 2)
            });

            _orderRepositoryMock.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingOrder);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Duplicate");

            _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithException_ShouldRollbackAndReturnFailureResponse()
        {
            // Arrange
            var command = new RegisterOrderCommand
            {
                ExternalId = "EXT-123",
                Products = new List<ProductDto>
                {
                    new ProductDto { Name = "Product 1", Price = 100, Quantity = 2 }
                }
            };

            _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Error");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
