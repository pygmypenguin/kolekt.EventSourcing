using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Consumers
{
    public abstract class EventHandlerBase<TEvent> : Consumer<TEvent> where TEvent : Event
    {
    }
}
