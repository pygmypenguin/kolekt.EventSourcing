using kolekt.EventSourcing.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Consumers
{
    /// <summary>
    /// For use when the caller won't expect any response back
    /// </summary>
    /// <typeparam name="TCommand">Type of command to handle</typeparam>
    public abstract class CommandHandlerBase<TCommand> : Consumer<TCommand>
        where TCommand : Command
    {
    }

    /// <summary>
    /// For use when the caller is expecting some kind of response, such as a model with an updated state
    /// </summary>
    /// <typeparam name="TCommand">Type of command to handle</typeparam>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    public abstract class CommandHandlerBase<TCommand, TResponse> : Consumer<TCommand> 
        where TCommand : Command 
        where TResponse : class
    {
        protected virtual void Succeed(TResponse value)
        {
            if (Context != null)
            {
                var response = new CommandResponse<TResponse>
                {
                    Data = value,
                    Success = true
                };
                Context.Respond(response);
            }
        }

        protected virtual void Fail(string message)
        {
            if (Context != null)
            {
                var response = new CommandResponse<TResponse>
                {
                    ErrorMessage = message,
                    Success = false
                };
                Context.Respond(response);
            }
        }
    }
}
