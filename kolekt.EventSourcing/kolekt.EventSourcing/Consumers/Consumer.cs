using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Consumers
{
    public abstract class Consumer<TMessage> : IConsumer<TMessage> where TMessage : Message 
    {
        protected ConsumeContext<TMessage> Context { get; private set; }

        public Task Consume(ConsumeContext<TMessage> context)
        {
            Context = context;
            var command = context.Message;
            return HandleAsync(command);
        }

        public abstract Task HandleAsync(TMessage message);
    }
}
