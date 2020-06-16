using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Exceptions
{
    public class CommandValidationException : Exception
    {
        public CommandValidationException(string message) : base(message)
        {
        }
    }
}
