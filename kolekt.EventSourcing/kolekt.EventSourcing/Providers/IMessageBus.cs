using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using MassTransit;
using System.Threading;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Providers
{
    public interface IMessageBus
    {
        /// <summary>
        /// Publish an event using a raw event bus. Only use if your provider is initiating a business operation.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event to publish</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="massTransitBus">The that will provide the endpoint context</param>
        /// <param name="shouldPublish">Publish the event to listeners that aren't on the exact queue name</param>
        /// <param name="cancellationToken"></param>
        Task<TAggregate> SendCommandAsync<TCommand, TAggregate>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default)
            where TCommand : Command 
            where TAggregate : AggregateRoot;

        /// <summary>
        /// Publish an event with contextual information surrounding the origin of the event. Use when possible
        /// </summary>
        /// <typeparam name="TEvent">Type of the event to publish</typeparam>
        /// <param name="eventContext">Consumer context to enable message tracing through the system</param>
        /// <param name="event">The event to publish</param>
        /// <param name="shouldPublish">Publish the event to listeners that aren't on the exact queue name</param>
        /// <param name="cancellationToken"></param>
        Task PublishEventAsync<TEvent>(TEvent @event, ConsumeContext eventContext, CancellationToken cancellationToken = default) 
            where TEvent : Event;
    }
}
