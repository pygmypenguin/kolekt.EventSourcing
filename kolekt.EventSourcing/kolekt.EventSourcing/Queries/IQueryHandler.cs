using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Queries
{
    public interface IQueryHandler<TQuery, TResponse>
    {
        Task<TResponse> QueryAsync(TQuery query);
    }
}
