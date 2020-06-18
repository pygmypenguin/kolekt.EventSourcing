using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace kolekt.EventSourcing.Aggregates
{
    public class AggregateRepository<TAggregateRoot> : IAggregateRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IEventStore _eventStore;
        private readonly IMemoryCache _memoryCache;

        private const int _cacheTtlMinutes = 5;

        public AggregateRepository(IEventStore eventStore, IMemoryCache memoryCache)
        {
            _eventStore = eventStore;
            _memoryCache = memoryCache;
        }

        public async Task<TAggregateRoot> FindById(Guid aggregateRootId, bool useCache = true)
        {
            if (useCache && _memoryCache.TryGetValue(aggregateRootId, out var cachedAggregate))
            {
                return cachedAggregate as TAggregateRoot;
            }

            var events = await _eventStore.LoadEventsAsync(aggregateRootId);
            if (events.Any())
            {
                var root = Activator.CreateInstance(typeof(TAggregateRoot), new object[] { aggregateRootId }) as TAggregateRoot;
                await root.RehydrateAsync(events);
                _memoryCache.Set(root.Id, root, TimeSpan.FromMinutes(_cacheTtlMinutes));
                return root;
            }
            else
            {
                return null;
            }
        }

        public async Task Save(TAggregateRoot aggregateRoot)
        {
            var events = aggregateRoot.UncommittedEvents;
            (var success, var newVersion) = await _eventStore.SaveEventsAsync(aggregateRoot.Id, aggregateRoot.CurrentVersion, events, aggregateRoot.GetType().Name);

            if (success)
            {
                await aggregateRoot.CommitEventsAsync(newVersion);
                _memoryCache.Set(aggregateRoot.Id, aggregateRoot, TimeSpan.FromMinutes(_cacheTtlMinutes));
            }
        }

        public Task Remove(Guid aggregateRootId)
        {
            _memoryCache.Remove(aggregateRootId);
            return _eventStore.DeleteEventsAsync(aggregateRootId);
        }
    }
}
