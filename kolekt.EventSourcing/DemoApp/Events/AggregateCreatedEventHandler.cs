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
        public AggregateCreatedEventHandler()
        {
        }

        public override Task HandleAsync(AggregateCreatedEvent message)
        {
            Console.WriteLine($"created: {message.DemoId}");
            return Task.CompletedTask;
        }
    }
}
