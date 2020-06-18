using DemoApp.Aggregates;
using DemoApp.Messages;
using kolekt.EventSourcing;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DemoApp.Events
{
    public class DemoDeletedEventHandler : EventHandlerBase<DemoDeletedEvent>
    {
        private readonly ILogger _logger;
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;

        public DemoDeletedEventHandler(IAggregateRepository<DemoAggregate> aggregateRepository, ILogger<DemoDeletedEventHandler> logger)
        {
            _aggregateRepository = aggregateRepository;
            _logger = logger;
        }

        public override async Task HandleAsync(DemoDeletedEvent message)
        {
            try 
            { 
                await _aggregateRepository.Remove(message.DemoId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error deleting DemoAggregate events from event store.", message.DemoId, message.EventId);
            }
        }
    }
}
