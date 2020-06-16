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
        Task<TAggregateRoot> FindById(Guid aggregateRootId);

        /// <summary>
        /// Save events applied to aggregate root in event store
        /// </summary>
        /// <param name="aggregateRoot">Aggregate root object to save</param>
        /// <returns></returns>
        Task Save(TAggregateRoot aggregateRoot);
    }
}
