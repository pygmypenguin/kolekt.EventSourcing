using kolekt.EventSourcing.Messages;
using System;

namespace DemoApp.Messages
{
    public class DemoDeletedEvent : Event
    {
        public Guid DemoId { get; set; }
    }
}
