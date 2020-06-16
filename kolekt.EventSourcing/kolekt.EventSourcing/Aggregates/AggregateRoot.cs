using Automatonymous;
using kolekt.EventSourcing.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Event = kolekt.EventSourcing.Messages.Event;

namespace kolekt.EventSourcing.Aggregates
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        public int CurrentVersion { get; internal set; }

        public Guid Id { get; private set; }

        private readonly Queue<(ConsumeContext, Event)> _uncommitedEvents;

        internal IReadOnlyCollection<(ConsumeContext, Event)> UncommittedEvents => _uncommitedEvents;

        public AggregateRoot(Guid id)
        {
            Id = id;
            _uncommitedEvents = new Queue<(ConsumeContext, Event)>();
        }

        public async Task RehydrateAsync(IReadOnlyCollection<object> events)
        {
            foreach (var e in events)
            {
                await ApplyEventAsync(e);
            }
        }

        protected async Task ApplyEventAsync<TEvent>(ConsumeContext context, TEvent domainEvent, bool shouldPersistEvent = true) where TEvent : Event
        {
            await ApplyEventAsync(domainEvent);

            if (shouldPersistEvent)
            {
                _uncommitedEvents.Enqueue((context, domainEvent));
            }
        }

        internal Task ApplyEventAsync<TEvent>(TEvent domainEvent) where TEvent : class
        {
            var t = Task.Factory.StartNew(() =>
            {
                var eventType = domainEvent.GetType();
                var eventApplicator = this.GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).AsEnumerable();
                eventApplicator = eventApplicator.Where(a => a.Name == "Apply");
                eventApplicator = eventApplicator.Where(a => a.GetParameters().Count() == 1 && a.GetParameters().First().ParameterType.IsAssignableFrom(eventType));
                eventApplicator = eventApplicator.Where(a => a.ReturnType.IsAssignableFrom(typeof(void)));

                //.Where(a => a.Name == "Apply")
                //.Where(a => a.GetParameters().Count() == 1 && a.GetParameters().First().ParameterType.IsAssignableFrom(eventType))
                //.Where(a => a.ReturnType.IsAssignableFrom(typeof(void)))
                //.SingleOrDefault();
                var evt = eventApplicator.SingleOrDefault();
                if (evt != null)
                {
                    evt.Invoke(this, new object[] { domainEvent });
                }

                //even if we don't do anything, update the version since an event definitely happened
                CurrentVersion++;
            });

            return t;
        }

        internal Task CommitEventsAsync()
        {
            _uncommitedEvents.Clear();
            return Task.CompletedTask;
        }
    }
}
