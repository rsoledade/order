using Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Messaging
{
    public class MockEventPublisher : IEventPublisher
    {
        private readonly ILogger<MockEventPublisher> _logger;

        public MockEventPublisher(ILogger<MockEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
        {
            _logger.LogInformation("Event published: {EventType}. Content: {@EventContent}",
                typeof(TEvent).Name, @event);

            return Task.CompletedTask;
        }
    }
}
