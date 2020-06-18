using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Consumers
{
    public class CommandResponse<TData>
    {
        public TData Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
