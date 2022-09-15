using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using InitQ.Cache;
using InitQ.Model;

namespace InitQTest.Work
{
    public class HostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;

        public HostedService(ILogger<HostedService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //测试定时
            Task.Run(async () =>
            {
                using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var _redisService = scope.ServiceProvider.GetService<ICacheService>();
                    await _redisService.ListLeftPushAsync<IntervalMessage>("tibos_interval_test_1", new IntervalMessage() { Msg = "6666666666666" });
                };
            });

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("内部任务计划结束");
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
