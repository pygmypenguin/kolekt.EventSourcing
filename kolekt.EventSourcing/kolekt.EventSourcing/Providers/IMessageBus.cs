using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using kolekt.EventSourcing.Consumers;
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
        /// <typeparam name="TCommand">Type of the event to publish</typeparam>
        /// <param name="command">The event to publish</param>
        /// <param name="commandContext">The context of the command being sent.</param>
        /// <param name="cancellationToken"></param>
        Task SendCommandAsync<TCommand>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default)
            where TCommand : Command;

        /// <summary>
        /// Publish an event using a raw event bus. Only use if your provider is initiating a business operation.
        /// </summary>
        /// <typeparam name="TCommand">Type of the event to publish</typeparam>
        /// <param name="command">The event to publish</param>
        /// <param name="commandContext">The context of the command being sent.</param>
        /// <param name="cancellationToken"></param>
        Task<CommandResponse<TAggregate>> SendCommandAsync<TCommand, TAggregate>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default)
            where TCommand : Command 
            where TAggregate : AggregateRoot;

        /// <summary>
        /// Publish an event with contextual information surrounding the origin of the event. Use when possible
        /// </summary>
        /// <typeparam name="TEvent">Type of the event to publish</typeparam>
        /// <param name="eventContext">Consumer context to enable message tracing through the system</param>
        /// <param name="event">The event to publish</param>
        /// <param name="cancellationToken"></param>
        Task PublishEventAsync<TEvent>(TEvent @event, ConsumeContext eventContext, CancellationToken cancellationToken = default) 
            where TEvent : Event;
    }
}
