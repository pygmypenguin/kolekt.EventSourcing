using DemoApp.Aggregates;
using DemoApp.Exceptions;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class CreateAggregateCommandHandler : CommandHandlerBase<CreateAggregateCommand>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;

        public CreateAggregateCommandHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public override async Task HandleAsync(CreateAggregateCommand message)
        {
            var aggregate = new DemoAggregate(message.DemoId);

            await aggregate.OnCreated(Context);
            await Context.RespondAsync(aggregate);

            await _aggregateRepository.Save(aggregate);
        }
    }
}
