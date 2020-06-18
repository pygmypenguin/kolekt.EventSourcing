using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using MassTransit;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace kolekt.EventSourcing.UnitTests
{
    public class AggregateRepositoryTests
    {
        private readonly Mock<IEventStore> _eventStore;
        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly Guid _validAggregate = Guid.NewGuid();

        private readonly AggregateRepository<MockAggregate> _target;

        private delegate void GetCacheValueCallback(object key, out object value);

        public AggregateRepositoryTests()
        {
            _eventStore = new Mock<IEventStore>();
            _memoryCache = new Mock<IMemoryCache>();

            _target = new AggregateRepository<MockAggregate>(_eventStore.Object, _memoryCache.Object);
        }

        [Fact]
        public async Task Given_AggregateIdIsValid_And_AggregateNotCached_When_FindById_Then_AggregateReturnedFromStore()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(_validAggregate))
                .ReturnsAsync(new List<Event>() { new MockEvent() }.ToImmutableList())
                .Verifiable();

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);

            var aggregate = await _target.FindById(_validAggregate);

            _eventStore.Verify(a => a.LoadEventsAsync(_validAggregate), Times.Once());
            Assert.NotNull(aggregate);
        }

        [Fact]
        public async Task Given_AggregateIdIsValid_And_AggregateNotCached_When_FindById_Then_CacheIsSet()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(_validAggregate))
                .ReturnsAsync(new List<Event>() { new MockEvent() }.ToImmutableList());

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object)
                .Verifiable();

            var aggregate = await _target.FindById(_validAggregate);

            _memoryCache.Verify(a => a.CreateEntry(It.IsAny<object>()), Times.Once());
            Assert.NotNull(aggregate);
        }

        [Fact]
        public async Task Given_AggregateIdIsValid_And_AggregateNotCached_When_FindById_Then_CacheValuesAreCorrect()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(_validAggregate))
                .ReturnsAsync(new List<Event>() { new MockEvent() }.ToImmutableList());

            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.SetupAllProperties();

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry.Object);

            var aggregate = await _target.FindById(_validAggregate);

            Assert.Equal(TimeSpan.FromMinutes(5), cacheEntry.Object.AbsoluteExpirationRelativeToNow);
            Assert.Equal(aggregate, cacheEntry.Object.Value);
        }

        [Fact]
        public async Task Given_AggregateDoesNotExistInStore_And_AggregateNotCached_When_FindById_Then_NullReturned()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<Event>().ToImmutableList())
                .Verifiable();

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);

            var aggregate = await _target.FindById(_validAggregate);

            _eventStore.Verify(a => a.LoadEventsAsync(It.IsAny<Guid>()), Times.Once());
            Assert.Null(aggregate);
        }

        [Fact]
        public async Task Given_AggregateDoesNotExistInStore_And_AggregateNotCached_When_FindById_Then_CacheNotSet()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<Event>().ToImmutableList());

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object)
                .Verifiable();

            var aggregate = await _target.FindById(_validAggregate);

            _memoryCache.Verify(a => a.CreateEntry(It.IsAny<object>()), Times.Never());
            Assert.Null(aggregate);
        }

        [Fact]
        public async Task Given_AggregateIsCached_When_FindByIdWithCache_Then_CacheValueIsReturned()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(It.IsAny<Guid>()))
                .Verifiable();

            _memoryCache.Setup(a => a.TryGetValue(_validAggregate as object, out It.Ref<object>.IsAny))
                .Callback(new GetCacheValueCallback((object key, out object a) => { a = new MockAggregate(Guid.NewGuid()); }))
                .Returns(true);

            var aggregate = await _target.FindById(_validAggregate);

            Assert.NotNull(aggregate);
            _eventStore.Verify(a => a.LoadEventsAsync(It.IsAny<Guid>()), Times.Never());
        }

        [Fact]
        public async Task Given_AggregateIsCached_When_FindByIdWithNoCache_Then_EventStoreValueIsReturned()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Event>() { new MockEvent() }.ToImmutableList())
                .Verifiable();

            _memoryCache.Setup(a => a.TryGetValue(_validAggregate as object, out It.Ref<object>.IsAny))
                .Verifiable();

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);

            var aggregate = await _target.FindById(_validAggregate, false);

            Assert.NotNull(aggregate);
            _eventStore.Verify(a => a.LoadEventsAsync(It.IsAny<Guid>()), Times.Once());
            _memoryCache.Verify(a => a.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Never());
        }

        [Fact]
        public async Task Given_AggregateIsValid_When_FindById_And_AggregateNotCached_Then_TryFindInCacheFirst()
        {
            _eventStore.Setup(a => a.LoadEventsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Event>() { new MockEvent() }.ToImmutableList())
                .Verifiable();

            _memoryCache.Setup(a => a.TryGetValue(_validAggregate as object, out It.Ref<object>.IsAny))
                .Returns(false)
                .Verifiable();

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);

            var aggregate = await _target.FindById(_validAggregate);

            Assert.NotNull(aggregate);
            _eventStore.Verify(a => a.LoadEventsAsync(It.IsAny<Guid>()), Times.Once());
            _memoryCache.Verify(a => a.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once());
        }

        [Fact]
        public async Task Given_AggregateHasEvents_When_SavingAggregate_Then_DataIsSaved()
        {
            var context = new Mock<ConsumeContext>();
            var aggregateToSave = new MockAggregate(Guid.NewGuid());
            await aggregateToSave.OnMockCommand(context.Object);

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object);

            _eventStore.Setup(a => a.SaveEventsAsync(It.IsAny<Guid>(), 0, It.IsAny<IReadOnlyCollection<(ConsumeContext, Event)>>(), It.IsAny<string>()))
                .ReturnsAsync((true, 3));

            await _target.Save(aggregateToSave);

            Assert.Equal(3, aggregateToSave.CurrentVersion);
        }

        [Fact]
        public async Task Given_AggregateHasEvents_When_SavingAggregate_Then_CacheIsSet()
        {
            var context = new Mock<ConsumeContext>();
            var aggregateToSave = new MockAggregate(Guid.NewGuid());
            await aggregateToSave.OnMockCommand(context.Object);

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object)
                .Verifiable();

            _eventStore.Setup(a => a.SaveEventsAsync(It.IsAny<Guid>(), 0, It.IsAny<IReadOnlyCollection<(ConsumeContext, Event)>>(), It.IsAny<string>()))
                .ReturnsAsync((true, 3));

            await _target.Save(aggregateToSave);

            _memoryCache.Verify(a => a.CreateEntry(It.IsAny<object>()), Times.Once());
        }


        [Fact]
        public async Task Given_AggregateHasEvents_When_SavingFails_Then_CacheIsNotSet()
        {
            var context = new Mock<ConsumeContext>();
            var aggregateToSave = new MockAggregate(Guid.NewGuid());
            await aggregateToSave.OnMockCommand(context.Object);

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object)
                .Verifiable();

            _eventStore.Setup(a => a.SaveEventsAsync(It.IsAny<Guid>(), 0, It.IsAny<IReadOnlyCollection<(ConsumeContext, Event)>>(), It.IsAny<string>()))
                .ReturnsAsync((false, 3));

            await _target.Save(aggregateToSave);

            _memoryCache.Verify(a => a.CreateEntry(It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task Given_AggregateHasEvents_When_SavingFails_Then_AggregateEventsNotCommitted()
        {
            var context = new Mock<ConsumeContext>();
            var aggregateToSave = new MockAggregate(Guid.NewGuid());
            await aggregateToSave.OnMockCommand(context.Object);

            _memoryCache.Setup(a => a.CreateEntry(It.IsAny<object>()))
                .Returns(new Mock<ICacheEntry>().Object)
                .Verifiable();

            _eventStore.Setup(a => a.SaveEventsAsync(It.IsAny<Guid>(), 0, It.IsAny<IReadOnlyCollection<(ConsumeContext, Event)>>(), It.IsAny<string>()))
                .ReturnsAsync((false, 3));

            await _target.Save(aggregateToSave);

            Assert.Equal(0, aggregateToSave.CurrentVersion);
        }

        [Fact]
        public async Task When_AggregateRemoved_Then_AggregateRemovedFromCache()
        {
            _memoryCache.Setup(a => a.Remove(It.IsAny<object>()))
                .Verifiable();

            await _target.Remove(_validAggregate);

            _memoryCache.Verify(a => a.Remove(It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async Task When_AggregateRemoved_Then_EventsRemovedFromEventStore()
        {
            _eventStore.Setup(a => a.DeleteEventsAsync(It.IsAny<Guid>()))
                .Verifiable();

            await _target.Remove(_validAggregate);

            _eventStore.Verify(a => a.DeleteEventsAsync(It.IsAny<Guid>()), Times.Once());
        }
    }
}
