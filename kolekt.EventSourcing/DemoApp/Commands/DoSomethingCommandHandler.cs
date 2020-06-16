using DemoApp.Aggregates;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class DoSomethingCommandHandler : CommandHandlerBase<DoSomethingCommand>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;
        public DoSomethingCommandHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public override async Task HandleAsync(DoSomethingCommand message)
        {
            var aggregate = await _aggregateRepository.FindById(message.DemoId);
            await aggregate.OnDoSomething(Context);
            await _aggregateRepository.Save(aggregate);
            await Context.RespondAsync(aggregate);
        }
    }
}
