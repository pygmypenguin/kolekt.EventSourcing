using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Aggregates
{
    public class AggregateConcurrencyException : Exception
    {
        private const string _defaultMessage = "Concurrency exception when attempting to write data for aggregate";

        public Guid AggregateId { get; private set; }

        public AggregateConcurrencyException(Guid aggregateId) : base(_defaultMessage)
        {
            AggregateId = aggregateId;
        }

        public AggregateConcurrencyException(Guid aggregateId, string message) : base(message ?? _defaultMessage)
        {
            AggregateId = aggregateId;
        }

        public AggregateConcurrencyException(Guid aggregateId, string message, Exception innerException) : base(message ?? _defaultMessage, innerException)
        {
            AggregateId = aggregateId;
        }
    }
}
