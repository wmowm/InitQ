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

            services.AddHostedService<InitQBackgroundService>();


            if (options.ListSubscribe != null)
            {
                foreach (var item in options.ListSubscribe)
                {
                    services.TryAddSingleton(item);
                }

                services.AddSingleton(serviceProvider =>
                {
                    Func<Type, IRedisSubscribe> accesor = key =>
                    {
                        foreach (var item in options.ListSubscribe)
                        {
                            if (key == item)
                            {
                                return serviceProvider.GetService(item) as IRedisSubscribe;
                            }
                        }
                        throw new ArgumentException($"不支持的DI Key: {key}");
                    };
                    return accesor;
                });
            }

            return new InitQBuilder(services);
        }
    }
}
