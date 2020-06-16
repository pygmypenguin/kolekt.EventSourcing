using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Aggregates
{
    public interface IAggregateRoot
    {
        Guid Id { get; }

        Task RehydrateAsync(IReadOnlyCollection<object> events);
    }
}
