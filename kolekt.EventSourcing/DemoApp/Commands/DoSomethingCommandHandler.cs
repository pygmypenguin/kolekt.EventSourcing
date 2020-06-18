using DemoApp.Aggregates;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace DemoApp.Messages
{
    public class DoSomethingCommandHandler : CommandHandlerBase<DoSomethingCommand, DemoAggregate>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;
        public DoSomethingCommandHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public override Task HandleAsync(DoSomethingCommand message)
        {
            return HandleWithRetry(message, 1);
        }

        private async Task HandleWithRetry(DoSomethingCommand message, int attempt)
        {
            try
            {
                var aggregate = await _aggregateRepository.FindById(message.DemoId, useCache: attempt == 1);
                await aggregate.OnDoSomething(Context);
                await _aggregateRepository.Save(aggregate);
                Succeed(aggregate);
            }
            catch (AggregateConcurrencyException)
            {
                if (attempt < 2)
                {
                    await HandleWithRetry(message, ++attempt);
                }
                else
                {
                    Fail($"Entity could not be updated: State out of sync. Please try again. ({message.CommandId})");
                }
            }
            catch(Exception e)
            {
                Fail(e.Message);
            }
        }
    }
}
