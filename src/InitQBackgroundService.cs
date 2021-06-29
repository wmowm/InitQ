using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InitQ
{
    public class InitQBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IOptions<InitQOptions> _options;
        private readonly IServiceProvider _provider;

        public InitQBackgroundService(ILogger<InitQBackgroundService> logger, IServiceProvider provider, IOptions<InitQOptions> options)
        {
            _logger = logger;
            _provider = provider;
            _options = options;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("InitQ Background Service Stopping.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InitQ Background Service Starting.");
            var init = new InitQCore();
            await init.FindInterfaceTypes(_provider, _options.Value, stoppingToken);
        }
    }
}