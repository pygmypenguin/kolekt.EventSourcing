using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Aggregates
{
    public class AggregateConcurrencyException : Exception
    {
        public Guid AggregateId { get; private set; }

        public AggregateConcurrencyException(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
