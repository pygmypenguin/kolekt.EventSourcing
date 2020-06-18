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
    public class CreateAggregateCommandHandler : CommandHandlerBase<CreateAggregateCommand, DemoAggregate>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;

        public CreateAggregateCommandHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public override async Task HandleAsync(CreateAggregateCommand message)
        {
            try
            {
                var aggregate = new DemoAggregate(message.DemoId);
                await aggregate.OnCreated(Context);
                await _aggregateRepository.Save(aggregate);

                Succeed(aggregate);
            }
            catch (Exception e)
            {
                Fail(e.Message);
            }
        }
    }
}
