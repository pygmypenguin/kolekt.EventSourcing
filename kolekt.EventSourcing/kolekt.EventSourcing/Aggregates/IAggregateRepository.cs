using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Aggregates
{
    public interface IAggregateRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        /// <summary>
        /// Fetch aggregate root by ID from event store
        /// </summary>
        /// <param name="aggregateRootId">ID of the aggregate root to find</param>
        /// <returns>Aggregate root with provided ID. Null if not found.</returns>
        Task<TAggregateRoot> FindById(Guid aggregateRootId, bool useCache = true);

        /// <summary>
        /// Save events applied to aggregate root in event store
        /// </summary>
        /// <param name="aggregateRoot">Aggregate root object to save</param>
        /// <returns></returns>
        Task Save(TAggregateRoot aggregateRoot);

        /// <summary>
        /// Delete the target aggregate from the event store and remove any cached references
        /// </summary>
        /// <param name="id">ID of the aggregate root to remove</param>
        /// <returns></returns>
        Task Remove(Guid id);
    }
}
