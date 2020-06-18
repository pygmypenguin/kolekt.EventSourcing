using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
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
        private readonly IClientFactory _clientFactory;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public MessageBus(IClientFactory clientFactory, ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _clientFactory = clientFactory;
        }
        public async Task SendCommandAsync<TCommand>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default)
            where TCommand : Command
        {
            var uri = GetSendUri(command);

            var sendEndpointTask = commandContext == null ? _sendEndpointProvider.GetSendEndpoint(uri) : commandContext.GetSendEndpoint(uri);
            var sendEndpoint = await sendEndpointTask;

            await sendEndpoint.Send(command, cancellationToken);
        }

        public async Task<CommandResponse<TAggregate>> SendCommandAsync<TCommand, TAggregate>(TCommand command, ConsumeContext commandContext = null, CancellationToken cancellationToken = default) 
            where TCommand : Command
            where TAggregate : AggregateRoot
        {
            var uri = GetSendUri(command);

            var requestClient = commandContext == null ? _clientFactory.CreateRequestClient<TCommand>(uri) : _clientFactory.CreateRequestClient<TCommand>(commandContext, uri);
            var requestHandle = requestClient.Create(command, cancellationToken);

            var response = await requestHandle.GetResponse<CommandResponse<TAggregate>>();
            return response.Message;
        }

        public async Task PublishEventAsync<TEvent>(TEvent @event, ConsumeContext eventContext, CancellationToken cancellationToken = default) 
            where TEvent : Event
        {
            await eventContext.Publish((object)@event, cancellationToken);
        }

        private Uri GetSendUri<TMessage>(TMessage message)
        {
            return new Uri($"queue:{message.GetType().Name}Handler");
        }
    }
}
