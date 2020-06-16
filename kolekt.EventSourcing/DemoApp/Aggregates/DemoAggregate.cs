using DemoApp.Messages;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Aggregates
{
    public class DemoAggregate : AggregateRoot
    {
        public string EventMessage { get; private set; }
        public DemoAggregate(Guid id) : base(id)
        {
        }

        public Task OnDoSomething(ConsumeContext<DoSomethingCommand> context)
        {
            return ApplyEventAsync(context, new SomethingHappenedEvent
            {
                DemoId = context.Message.DemoId,
                MessageText = context.Message.MessageText
            });
        }

        public Task OnCreated(ConsumeContext<CreateAggregateCommand> context)
        {
            return ApplyEventAsync(context, new AggregateCreatedEvent
            {
                DemoId = Id
            });
        }

        private void Apply(SomethingHappenedEvent @event)
        {
            EventMessage = @event.MessageText;
        }

        private void Apply(AggregateCreatedEvent @event)
        {
            //maybe do something here, idk
            return;
        }
    }
}
