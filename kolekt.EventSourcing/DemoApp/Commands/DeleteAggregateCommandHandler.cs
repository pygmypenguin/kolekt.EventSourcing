using DemoApp.Aggregates;
using DemoApp.Messages;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using System.Threading.Tasks;

namespace DemoApp.Commands
{
    public class DeleteAggregateCommandHandler : CommandHandlerBase<DeleteAggregateCommand>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;
        public DeleteAggregateCommandHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public override async Task HandleAsync(DeleteAggregateCommand message)
        {
            var aggregate = await _aggregateRepository.FindById(message.DemoId);
            await aggregate.OnDeleted(Context);
            await _aggregateRepository.Save(aggregate);
        }
    }
}
