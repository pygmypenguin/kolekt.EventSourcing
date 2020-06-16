using kolekt.EventSourcing.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Consumers
{
    public abstract class CommandHandlerBase<TCommand> : Consumer<TCommand> where TCommand : Command
    {
    }
}
