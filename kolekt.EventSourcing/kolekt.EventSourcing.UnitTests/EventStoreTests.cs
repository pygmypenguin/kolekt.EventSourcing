using kolekt.EventSourcing.Messages;
using kolekt.EventSourcing.Providers;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace kolekt.EventSourcing.UnitTests
{
    public class EventStoreTests : IClassFixture<EventStoreDbFixture>
    {
        private readonly Mock<IMessageBus> _messageBus;
        private readonly EventStore _target;

        private readonly Guid _knownAggregateId;

        public EventStoreTests(EventStoreDbFixture fixture)
        {
            _messageBus = new Mock<IMessageBus>();
            _knownAggregateId = fixture.AggregateId;

            _target = new EventStore(fixture.DbContext, _messageBus.Object);
        }

        [Fact]
        public async Task Given_AggregateHasEvents_When_GettingEvents_Then_EventsAreReturned()
        {
            var events = await _target.LoadEventsAsync(_knownAggregateId);

            Assert.True(events.Any());
            Assert.All(events, a => Assert.NotNull(a as Event));
        }

        [Fact]
        public async Task Given_AggregateHasEvents_When_GettingEvents_Then_EventsAreOrdered()
        {
            var events = await _target.LoadEventsAsync(_knownAggregateId);

            for (int i = 0; i < events.Count - 1; i++)
            {
                var a = events.ElementAt(i) as MockEvent;
                var b = events.ElementAt(i + 1) as MockEvent;

                Assert.True(a.Timestamp < b.Timestamp);
            }
        }

        [Fact]
        public async Task Given_AggregateHasNoEvent_When_GettingEvents_Then_EmptyListReturned()
        {
            var events = await _target.LoadEventsAsync(Guid.Empty);

            Assert.Empty(events);
        }
    }
}
