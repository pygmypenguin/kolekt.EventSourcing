using kolekt.EventSourcing.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class DeleteAggregateCommand : Command
    {
        public Guid DemoId { get; set; }
    }
}
