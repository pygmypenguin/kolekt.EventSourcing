using kolekt.EventSourcing.Messages;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Consumers
{
    public class CommandValidationResult
    {
        public static CommandValidationResult Valid = new CommandValidationResult { IsValid = true };
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string Property { get; set; }
    }

    public interface ICommandValidator
    {
        Task<CommandValidationResult> Validate(Command command);
    }

    public abstract class CommandValidatorBase : ICommandValidator
    {
        public Task<CommandValidationResult> Validate(Command command)
        {
            var type = command.GetType();
            var validateMethod = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(a => a.Name == nameof(Validate) &&
                    a.GetParameters().Length == 1 &&
                    a.GetParameters().First().ParameterType.IsAssignableFrom(type) &&
                    a.GetParameters().First().ParameterType != typeof(Command) &&
                    a.ReturnType.IsGenericType &&
                    a.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                    a.ReturnType.GetGenericArguments().First().IsAssignableFrom(typeof(CommandValidationResult)))
                .FirstOrDefault();

            if (validateMethod != null)
            {
                return (Task<CommandValidationResult>)validateMethod.Invoke(this, new object[] { command });
            }
            else
            {
                return Task.FromResult(CommandValidationResult.Valid);
            }
        }
    }
}
