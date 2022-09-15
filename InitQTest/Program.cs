using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using InitQ;
using InitQTest.Example;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InitQTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                        .ConfigureHostConfiguration(configHost =>
                        {
                            configHost.SetBasePath(Directory.GetCurrentDirectory());
                            //configHost.AddJsonFile("hostsettings.json", true, true);
                            configHost.AddEnvironmentVariables("ASPNETCORE_");
                            //configHost.AddCommandLine(args);
                        })
                        .ConfigureAppConfiguration((hostContext, configApp) =>
                        {

                            configApp.AddJsonFile("appsettings.json", true);
                            //configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                            configApp.AddEnvironmentVariables();
                            //configApp.AddCommandLine(args);

                        })
                        .ConfigureServices((hostContext, services) =>
                        {
                            
                            services.AddLogging();
                            services.AddHostedService<InitQTest.Work.HostedService>();
                            services.AddInitQ(m =>
                            {
                                m.SuspendTime = 1000;
                                m.ConnectionString = "127.0.0.1,connectTimeout=15000,syncTimeout=5000,password=123456";
                                m.ListSubscribe = new List<Type>() { typeof(RedisSubscribeA), typeof(RedisIntervalSubscribeA) };
                                m.ShowLog = false;
                            });
                        })
                        .ConfigureLogging((hostContext, configLogging) =>
                        {

                            configLogging.AddConsole();
                            if (hostContext.HostingEnvironment.EnvironmentName == EnvironmentName.Development)
                            {
                                configLogging.AddDebug();
                            }
                        })
                        .UseConsoleLifetime()
                        .Build();
            host.Run();
            Console.ReadLine();
        }
    }
}
