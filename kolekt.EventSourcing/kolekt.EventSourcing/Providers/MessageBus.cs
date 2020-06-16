using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Providers
{
    public class MessageBus : IMessageBus
    {
        private readonly IBus _bus;
        public MessageBus(IBus bus)
        {
            _bus = bus;
        }

        public async Task<TAggregate> SendCommandAsync<TCommand, TAggregate>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default) 
            where TCommand : Command
            where TAggregate : AggregateRoot
        {
            var uri = GetSendUri(command);

            var requestClient = commandContext == null ? _bus.CreateRequestClient<TCommand>(uri) : commandContext.CreateRequestClient<TCommand>(_bus, uri);
            var requestHandle = requestClient.Create(command, cancellationToken);

            var response = await requestHandle.GetResponse<TAggregate>();
            return response.Message;
        }

        public async Task PublishEventAsync<TEvent>(TEvent @event, ConsumeContext eventContext, CancellationToken cancellationToken = default) 
            where TEvent : Event
        {
            var uri = GetSendUri(@event);
            var endpoint = await eventContext.GetSendEndpoint(uri);
            await endpoint.Send((TEvent)@event, cancellationToken);
            //await eventContext.Publish(@event, cancellationToken);
        }

        private Uri GetSendUri<TMessage>(TMessage message)
        {
            return new Uri($"queue:{message.GetType().Name}Handler");
        }
    }
}
