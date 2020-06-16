using kolekt.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using kolekt.Data.Models;
using System.Text.Json;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using kolekt.EventSourcing.Providers;
using kolekt.EventSourcing.Messages;
using System.Collections.Immutable;
using kolekt.EventSourcing.Aggregates;

namespace kolekt.EventSourcing
{
    public class EventStore : IEventStore
    {
        private readonly EventStoreDataContext _dataContext;
        private readonly IMessageBus _messageBus;

        public EventStore(EventStoreDataContext dataContext, IMessageBus messageBus)
        {
            _dataContext = dataContext;
            _messageBus = messageBus;
        }

        public async Task<IReadOnlyCollection<object>> LoadEventsAsync(Guid aggregateId)
        {
            var eventEntities = await _dataContext.Events
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(a => a.Version)
                .AsNoTracking()
                .ToListAsync();

            var events = eventEntities.Select(TransformEventEntity);

            return events.ToImmutableList();
        }

        public async Task<(bool Success, int NewVersion)> SaveEventsAsync(Guid aggregateId, int originatingVersion, IReadOnlyCollection<(ConsumeContext Context, Event @Event)> events, string aggregateName)
        {
            var newVersion = originatingVersion;
            var eventEntities = events.Select(e =>
                new EventEntity
                {
                    AggregateId = aggregateId,
                    AggregateType = aggregateName,
                    Data = JsonSerializer.Serialize(e.Event, e.Event.GetType()),
                    Id = e.Event.EventId,
                    Name = $"{e.Event.GetType().FullName}, {e.Event.GetType().Assembly.FullName}",
                    Version = ++newVersion
                });

            using (var t = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    _dataContext.Events.AddRange(eventEntities);
                    await _dataContext.SaveChangesAsync();

                    await t.CommitAsync();
                }
                catch
                {
                    await t.RollbackAsync();
                    return (false, originatingVersion);
                }
            }

            foreach (var e in events)
            {
                await _messageBus.PublishEventAsync(e.Event, e.Context);
            }
            return (true, newVersion);
        }

        private object TransformEventEntity(EventEntity e)
        {
            var type = Type.GetType(e.Name);
            return JsonSerializer.Deserialize(e.Data, type);
        }
    }
}
