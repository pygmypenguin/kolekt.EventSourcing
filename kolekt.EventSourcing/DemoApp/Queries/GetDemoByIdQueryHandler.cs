using DemoApp.Aggregates;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Queries
{
    public class GetDemoByIdQueryHandler : IQueryHandler<GetDemoByIdQuery, DemoAggregate>
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;
        public GetDemoByIdQueryHandler(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }
        public Task<DemoAggregate> QueryAsync(GetDemoByIdQuery query)
        {
            return _aggregateRepository.FindById(query.DemoId);
        }
    }
}
