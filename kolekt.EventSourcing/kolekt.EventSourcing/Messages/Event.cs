using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Messages
{
    public abstract class Event : Message
    {
        public Guid EventId => Id;
    }
}
