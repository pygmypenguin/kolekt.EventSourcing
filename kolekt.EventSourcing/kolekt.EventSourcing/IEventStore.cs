using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing
{
    public interface IEventStore
    {
        /// <summary>
        /// Commit aggregate root events to the configured data store
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate root</param>
        /// <param name="originatingVersion">Version number of the last known aggregate version. Used to ensure concurrent accuracy</param>
        /// <param name="events">Collection of events to be saved</param>
        /// <param name="aggregateName">Name of the aggregate root type</param>
        /// <returns>true if successful, otherwise false</returns>
        Task<(bool Success, int NewVersion)> SaveEventsAsync(Guid aggregateId, int originatingVersion, IReadOnlyCollection<(ConsumeContext Context, Event @Event)> events, string aggregateName);

        /// <summary>
        /// Load events applied to a given aggregate root
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate root</param>
        /// <returns>A readonly collection of events that have been applied to the aggregate root</returns>
        Task<IReadOnlyCollection<object>> LoadEventsAsync(Guid aggregateId);

        /// <summary>
        /// Wipe the record of an entity ever having existed. Use with caution.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate root to remove</param>
        /// <returns></returns>
        Task<bool> DeleteEventsAsync(Guid aggregateId);
    }
}
