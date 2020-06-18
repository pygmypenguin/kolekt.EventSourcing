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
using Microsoft.Data.SqlClient;

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
            using (var trans = _dataContext.Database.BeginTransaction())
            {
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

                try
                {
                    _dataContext.Events.AddRange(eventEntities);
                    await _dataContext.SaveChangesAsync();

                    await trans.CommitAsync();
                }
                catch (DbUpdateException e) when (e.GetBaseException().GetType() == typeof(SqlException))
                {
                    trans.Rollback();
                    var baseException = e.GetBaseException();
                    if (baseException.Message.IndexOf("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new AggregateConcurrencyException(aggregateId, "Invalid state when saving entity data. Retry operation after rebuilding entity from saved events", e);
                    }
                    else
                    {
                        throw;
                    }

                }
                catch
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }

            foreach (var e in events)
            {
                await _messageBus.PublishEventAsync(e.Event, e.Context);
            }
            return (true, newVersion);
        }

        public async Task<bool> DeleteEventsAsync(Guid aggregateId)
        {
            using (var trans = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    var events = _dataContext.Events.Where(a => a.AggregateId == aggregateId);
                    _dataContext.Events.RemoveRange(events);
                    _dataContext.SaveChanges();
                    await trans.CommitAsync();

                    return true;
                }
                catch
                {
                    await trans.RollbackAsync();
                    return false;
                }
            }
        }

        private object TransformEventEntity(EventEntity e)
        {
            var type = Type.GetType(e.Name);
            return JsonSerializer.Deserialize(e.Data, type);
        }
    }
}
