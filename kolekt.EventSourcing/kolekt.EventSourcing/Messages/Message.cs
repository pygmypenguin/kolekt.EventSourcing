using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Messages
{
    public abstract class Message
    {
        protected virtual Guid Id { get; private set; } = Guid.NewGuid();
    }
}
