using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Messages
{
    public abstract class Event : Message
    {
        public Event()
        {
            EventId = Id;
        }
        public Guid EventId { get; set; }
    }
}
