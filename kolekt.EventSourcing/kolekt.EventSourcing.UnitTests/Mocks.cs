using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.UnitTests
{
    public class MockEvent : Event
    {
        public DateTime Timestamp { get; } = DateTime.Now;
    }

    public class MockCommand : Command 
    {
        public DateTime Timestam { get; } = DateTime.Now;
    }
    public class MockAggregate : AggregateRoot
    {
        public MockAggregate(Guid id) : base(id)
        {
        }

        public Task OnMockCommand(ConsumeContext context)
        {
            return ApplyEventAsync(context, new MockEvent());
        }
    }
}
