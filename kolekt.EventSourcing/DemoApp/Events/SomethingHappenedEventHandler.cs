using kolekt.EventSourcing.Consumers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class SomethingHappenedEventHandler : EventHandlerBase<SomethingHappenedEvent>
    {
        ILogger _logger;

        public SomethingHappenedEventHandler(ILogger<SomethingHappenedEventHandler> logger)
        {
            _logger = logger;
        }

        public override Task HandleAsync(SomethingHappenedEvent message)
        {
            _logger.LogInformation($"Received SomethingHappened event for entity {message.DemoId} with message {message.MessageText}");
            return Task.CompletedTask;
        }
    }
}
