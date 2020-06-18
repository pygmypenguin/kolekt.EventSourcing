using kolekt.EventSourcing.Consumers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class AggregateCreatedEventHandler : EventHandlerBase<AggregateCreatedEvent>
    {
        private readonly ILogger _logger;
        public AggregateCreatedEventHandler(ILogger<AggregateCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(AggregateCreatedEvent message)
        {
            _logger.LogInformation($"created: {message.DemoId}");
            return Task.CompletedTask;
        }
    }
}
