using MassTransit;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kolekt.EventSourcing.Providers
{
    public class BusService : IHostedService
    {
        private readonly IBusControl _busControl;

        public BusService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return _busControl.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return _busControl.StopAsync();
        }
    }
}
