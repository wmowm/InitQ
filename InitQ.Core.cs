using InitQ.Abstractions;
using InitQ.Cache;
using InitQ.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQ
{
    public class InitQCore
    {

        private void Send(IEnumerable<ConsumerExecutorDescriptor> ExecutorDescriptorList, IServiceProvider serviceProvider, InitQOptions options)
        {
            foreach (var ConsumerExecutorDescriptor in ExecutorDescriptorList)
            {
                //线程
                Task.Run(() =>
                {
                    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var publish = ConsumerExecutorDescriptor.Attribute.Name;
                        var provider = scope.ServiceProvider;
                        var obj = ActivatorUtilities.GetServiceOrCreateInstance(provider, ConsumerExecutorDescriptor.ImplTypeInfo);
                        ParameterInfo[] parameterInfos = ConsumerExecutorDescriptor.MethodInfo.GetParameters();
                        //redis对象
                        var _redis = scope.ServiceProvider.GetService<ICacheService>();
                        while (true)
                        {
                            try
                            {
                                if (options.ShowLog)
                                {
                                    Console.WriteLine($"执行方法:{obj.ToString()},key:{publish},执行时间{DateTime.Now}");
                                }
                                var count = _redis.ListLength(publish);
                                if (count > 0)
                                {
                                    //从MQ里获取一条消息
                                    var res = _redis.ListRightPop(publish);
                                    //堵塞
                                    Thread.Sleep(options.IntervalTime);
                                    try
                                    {
                                        Task.Run(() =>
                                        {
                                            if (parameterInfos.Length == 0)
                                            {
                                                ConsumerExecutorDescriptor.MethodInfo.Invoke(obj, null);
                                            }
                                            else
                                            {
                                                object[] parameters = new object[] { res };
                                                ConsumerExecutorDescriptor.MethodInfo.Invoke(obj, parameters);
                                            }
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else
                                {
                                    //线程挂起1s
                                    Thread.Sleep(options.SuspendTime);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                });
            }
        }


        public void FindInterfaceTypes(IServiceProvider provider, InitQOptions options)
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();
            using (var scoped = provider.CreateScope())
            {
                var scopedProvider = scoped.ServiceProvider;
                var list_service = scopedProvider.GetService<Func<Type, IRedisSubscribe>>();
                foreach (var item in options.ListSubscribe)
                {
                    var consumerServices = list_service(item);
                    var typeInfo = consumerServices.GetType().GetTypeInfo();
                    if (!typeof(IRedisSubscribe).GetTypeInfo().IsAssignableFrom(typeInfo))
                    {
                        continue;
                    }
                    executorDescriptorList.AddRange(GetTopicAttributesDescription(typeInfo));
                }
                //var consumerServices = scopedProvider.GetServices<IRedisSubscribe>();
                //foreach (var service in consumerServices)
                //{
                //    var typeInfo = service.GetType().GetTypeInfo();
                //    if (!typeof(IRedisSubscribe).GetTypeInfo().IsAssignableFrom(typeInfo))
                //    {
                //        continue;
                //    }
                //    executorDescriptorList.AddRange(GetTopicAttributesDescription(typeInfo));
                //}
                Send(executorDescriptorList, provider, options);
            }
        }

        private IEnumerable<ConsumerExecutorDescriptor> GetTopicAttributesDescription(TypeInfo typeInfo)
        {
            foreach (var method in typeInfo.DeclaredMethods)
            {
                var topicAttr = method.GetCustomAttributes<TopicAttribute>(true);
                var topicAttributes = topicAttr as IList<TopicAttribute> ?? topicAttr.ToList();

                if (!topicAttributes.Any())
                {
                    continue;
                }

                foreach (var attr in topicAttributes)
                {
                    yield return InitDescriptor(attr, method, typeInfo);
                }
            }
        }


        private ConsumerExecutorDescriptor InitDescriptor(TopicAttribute attr, MethodInfo methodInfo, TypeInfo implType)
        {
            var descriptor = new ConsumerExecutorDescriptor
            {
                Attribute = attr,
                MethodInfo = methodInfo,
                ImplTypeInfo = implType
            };

            return descriptor;
        }
    }
}
