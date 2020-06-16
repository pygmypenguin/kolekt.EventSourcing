using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Messages
{
    public abstract class Command : Message
    {
        public Guid CommandId => Id;
    }
}
