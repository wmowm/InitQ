using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQ
{
    public class HostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;
        private readonly IOptions<InitQOptions> _options;
        public HostedService(ILogger<HostedService> logger, IServiceProvider provider, IOptions<InitQOptions> options)
        {
            _logger = logger;
            _provider = provider;
            _options = options;
        }

        public  Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("程序启动");

            new InitQCore().FindInterfaceTypes(_provider, _options.Value);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("结束");


            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
