using InitQ.Abstractions;
using InitQ.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the consistence services for the consistency.
        /// </summary>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="CapOptions" />.</param>
        /// <returns>An <see cref="CapBuilder" /> for application services.</returns>
        public static InitQBuilder AddInitQ(this IServiceCollection services, Action<InitQOptions> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            var options = new InitQOptions();
            setupAction(options);


            services.Configure(setupAction);

            services.AddSingleton(typeof(ICacheService), new RedisCacheService(options.ConnectionString));

            services.AddHostedService<HostedService>();


            foreach (var item in options.ListSubscribe)
            {
                services.AddSingleton(item);
            }

            return new InitQBuilder(services);
        }
    }
}
