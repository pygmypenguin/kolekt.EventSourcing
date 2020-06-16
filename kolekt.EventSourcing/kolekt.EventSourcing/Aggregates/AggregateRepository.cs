using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace kolekt.EventSourcing.Aggregates
{
    public class AggregateRepository<TAggregateRoot> : IAggregateRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IEventStore _eventStore;

        public AggregateRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<TAggregateRoot> FindById(Guid aggregateRootId)
        {
            var events = await _eventStore.LoadEventsAsync(aggregateRootId);
            if (events.Any())
            {
                var root = Activator.CreateInstance(typeof(TAggregateRoot), new object[] { aggregateRootId }) as TAggregateRoot;
                await root.RehydrateAsync(events);
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
                aggregateRoot.CurrentVersion = newVersion;
                await aggregateRoot.CommitEventsAsync();
            }
            else
            {
                throw new AggregateConcurrencyException(aggregateRoot.Id);
            }

        }
    }
}
