using InitQ.Abstractions;
using InitQ.Attributes;
using InitQ.Cache;
using InitQ.Internal;
using InitQ.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
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

        private async Task Send(IEnumerable<ConsumerExecutorDescriptor> ExecutorDescriptorList, IServiceProvider serviceProvider, InitQOptions options)
        {
            List<Task> tasks = new List<Task>();
            foreach (var ConsumerExecutorDescriptor in ExecutorDescriptorList)
            {
                //线程
                tasks.Add(Task.Run(async() =>
                {
                    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var publish = ConsumerExecutorDescriptor.Attribute.Name;
                        var provider = scope.ServiceProvider;
                        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<InitQCore>();
                        var obj = ActivatorUtilities.GetServiceOrCreateInstance(provider, ConsumerExecutorDescriptor.ImplTypeInfo);
                        ParameterInfo[] parameterInfos = ConsumerExecutorDescriptor.MethodInfo.GetParameters();
                        //redis对象
                        var _redis = scope.ServiceProvider.GetService<ICacheService>();
                        while (true)
                        {
                            try
                            {
                                if (options.ShowLog && logger != null)
                                {
                                    logger.LogInformation($"执行方法:{obj.ToString()},key:{publish},执行时间{DateTime.Now}");
                                }
                                var count = await _redis.ListLengthAsync(publish);
                                if (count > 0)
                                {
                                    //从MQ里获取一条消息
                                    var res = await _redis.ListRightPopAsync(publish);
                                    if (string.IsNullOrEmpty(res)) continue;
                                    //堵塞
                                    await Task.Delay(options.IntervalTime);
                                    try
                                    {
                                        await Task.Run(async() =>
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
                                        await Task.Delay(TimeSpan.FromSeconds(1));
                                        logger.LogInformation(ex.Message);
                                    }
                                }
                                else
                                {
                                    //线程挂起1s
                                    await Task.Delay(options.SuspendTime);
                                }
                            }
                            catch (Exception ex)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                logger.LogInformation(ex.Message);
                            }
                        }
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }


        private async Task SendDelay(IEnumerable<ConsumerExecutorDescriptor> ExecutorDescriptorList, IServiceProvider serviceProvider, InitQOptions options)
        {
            List<Task> tasks = new List<Task>();
            foreach (var ConsumerExecutorDescriptor in ExecutorDescriptorList)
            {
                //线程
                tasks.Add(Task.Run(async () =>
                {
                    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var publish = $"queue:{ConsumerExecutorDescriptor.Attribute.Name}";
                        var provider = scope.ServiceProvider;
                        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<InitQCore>();
                        var obj = ActivatorUtilities.GetServiceOrCreateInstance(provider, ConsumerExecutorDescriptor.ImplTypeInfo);
                        ParameterInfo[] parameterInfos = ConsumerExecutorDescriptor.MethodInfo.GetParameters();
                        //redis对象
                        var _redis = scope.ServiceProvider.GetService<ICacheService>();

                        //从zset添加到队列(锁)
                        tasks.Add(Task.Run(async () =>
                        {
                            while (true)
                            {
                                try
                                {
                                    var keyInfo = "initq-lock:" + ConsumerExecutorDescriptor.Attribute.Name; //锁名称 每个延迟队列一个锁
                                    var token = Guid.NewGuid().ToString("N"); //锁持有者
                                    var coon = await _redis.GetDatabase().LockTakeAsync(keyInfo, token, TimeSpan.FromSeconds(5), CommandFlags.None);
                                    if (coon)
                                    {
                                        try
                                        {
                                            var dt = DateTime.Now;
                                            var arry = await _redis.SortedSetRangeByScoreAsync(ConsumerExecutorDescriptor.Attribute.Name, null, dt);
                                            if (arry != null && arry.Length > 0)
                                            {
                                                foreach (var item in arry)
                                                {
                                                    await _redis.ListLeftPushAsync(publish, item);
                                                }
                                                //移除zset数据
                                                await _redis.SortedSetRemoveRangeByScoreAsync(ConsumerExecutorDescriptor.Attribute.Name, null, dt);
                                            }
                                            else
                                            {
                                                //线程挂起1s
                                                await Task.Delay(1000);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            await Task.Delay(TimeSpan.FromSeconds(1));
                                            logger.LogInformation($"执行延迟队列报错:{ex.Message}");
                                        }
                                        finally
                                        {
                                            //释放锁
                                            await _redis.GetDatabase().LockReleaseAsync(keyInfo, token);
                                        }
                                    }
                                    else
                                    {
                                        //线程挂起10毫秒,避免循环竞争锁,造成开销
                                        await Task.Delay(10);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    logger.LogInformation(ex.Message);
                                }
                            }
                        }));
                        //消费队列
                        tasks.Add(Task.Run(async () => 
                        {
                            while (true)
                            {
                                try
                                {
                                    if (options.ShowLog && logger != null)
                                    {
                                        logger.LogInformation($"执行方法:{obj.ToString()},key:{publish},执行时间{DateTime.Now}");
                                    }
                                    var count = await _redis.ListLengthAsync(publish);
                                    if (count > 0)
                                    {
                                        //从MQ里获取一条消息
                                        var res = await _redis.ListRightPopAsync(publish);
                                        if (string.IsNullOrEmpty(res)) continue;
                                        //堵塞
                                        await Task.Delay(options.IntervalTime);
                                        try
                                        {
                                            await Task.Run(async () =>
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
                                            await Task.Delay(TimeSpan.FromSeconds(1));
                                            logger.LogInformation(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        //线程挂起1s
                                        await Task.Delay(options.SuspendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    logger.LogInformation(ex.Message);
                                }
                            }
                        }));
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }




        private async Task SendInterval(IEnumerable<ConsumerExecutorDescriptor> ExecutorDescriptorList, IServiceProvider serviceProvider, InitQOptions options)
        {
            List<Task> tasks = new List<Task>();
            foreach (var ConsumerExecutorDescriptor in ExecutorDescriptorList)
            {
                //线程
                tasks.Add(Task.Run(async () =>
                {
                    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var attribute = (SubscribeIntervalAttribute)ConsumerExecutorDescriptor.Attribute;

                        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<InitQCore>();
                        //取消息队列
                        var publish = ConsumerExecutorDescriptor.Attribute.Name;
                        //发消息队列
                        var push_queue = $"initq_interval_{ConsumerExecutorDescriptor.Attribute.Name}";
                        //延迟消息队列
                        var delay_queue = $"initq_delay_{ConsumerExecutorDescriptor.Attribute.Name}";

                        var provider = scope.ServiceProvider;
                        var obj = ActivatorUtilities.GetServiceOrCreateInstance(provider, ConsumerExecutorDescriptor.ImplTypeInfo);
                        ParameterInfo[] parameterInfos = ConsumerExecutorDescriptor.MethodInfo.GetParameters();
                        //redis对象
                        var _redis = scope.ServiceProvider.GetService<ICacheService>();

                        //从队列到zset
                        tasks.Add(Task.Run(async () => 
                        {
                            while (true)
                            {
                                try
                                {
                                    if (options.ShowLog && logger != null)
                                    {
                                        logger.LogInformation($"执行方法:{obj.ToString()},key:{publish},执行时间{DateTime.Now}");
                                    }
                                    var count = await _redis.ListLengthAsync(publish);
                                    if (count > 0)
                                    {
                                        //堵塞
                                        await Task.Delay(options.IntervalTime);

                                        //从MQ里获取一条消息
                                        var res = await _redis.ListRightPopAsync(publish);
                                        if (string.IsNullOrEmpty(res)) continue;
                                        var result = await IntervalPan(res, attribute, logger, _redis, options);
                                        if (string.IsNullOrEmpty(res)) continue;
                                        switch (result.Code) 
                                        {
                                            case 0:
                                                await _redis.ListLeftPushAsync(push_queue, result.Data);
                                                break;
                                            case -1:
                                                await _redis.ListLeftPushAsync(attribute.DeadLetterKey, result.Data);
                                                break;
                                            case -2:
                                                break;
                                            case 1:
                                                var dt = DateTime.Now.AddSeconds(Convert.ToInt32(result.Message));
                                                await _redis.SortedSetAddAsync(delay_queue, result.Data, dt);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //线程挂起1s
                                        await Task.Delay(options.SuspendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    logger.LogInformation(ex.Message);
                                }
                            }
                        }));
                        //从zset添加到队列(锁)
                        tasks.Add(Task.Run(async () =>
                        {
                            while (true)
                            {
                                try
                                {
                                    var keyInfo = "initq-interval-lock:" + ConsumerExecutorDescriptor.Attribute.Name; //锁名称 每个延迟队列一个锁
                                    var token = Guid.NewGuid().ToString("N"); //锁持有者
                                    var coon = await _redis.GetDatabase().LockTakeAsync(keyInfo, token, TimeSpan.FromSeconds(5), CommandFlags.None);
                                    if (coon)
                                    {
                                        try
                                        {
                                            var dt = DateTime.Now;
                                            var arry = await _redis.SortedSetRangeByScoreAsync(delay_queue, null, dt);
                                            if (arry != null && arry.Length > 0)
                                            {
                                                foreach (var item in arry)
                                                {
                                                    await _redis.ListLeftPushAsync(push_queue, item);
                                                }
                                                //移除zset数据
                                                await _redis.SortedSetRemoveRangeByScoreAsync(delay_queue, null, dt);
                                            }
                                            else
                                            {
                                                //线程挂起1s
                                                await Task.Delay(1000);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogInformation($"执行延迟队列报错:{ex.Message}");
                                        }
                                        finally
                                        {
                                            //释放锁
                                            await _redis.GetDatabase().LockReleaseAsync(keyInfo, token);
                                        }
                                    }
                                    else
                                    {
                                        //线程挂起10毫秒,避免循环竞争锁,造成开销
                                        await Task.Delay(10);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    logger.LogInformation(ex.Message);
                                }
                            }
                        }));
                        //消费队列
                        tasks.Add(Task.Run(async () =>
                        {
                            while (true)
                            {
                                try
                                {
                                    if (options.ShowLog && logger != null)
                                    {
                                        logger.LogInformation($"执行方法:{obj.ToString()},key:{push_queue},执行时间{DateTime.Now}");
                                    }
                                    var count = await _redis.ListLengthAsync(push_queue);
                                    if (count > 0)
                                    {
                                        //堵塞
                                        await Task.Delay(options.IntervalTime);
                                        //从MQ里获取一条消息
                                        var res = await _redis.ListRightPopAsync(push_queue);
                                        if (string.IsNullOrEmpty(res)) continue;
                                        try
                                        {
                                            await Task.Run(async () =>
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
                                            await Task.Delay(TimeSpan.FromSeconds(1));
                                            logger.LogInformation(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        //线程挂起1s
                                        await Task.Delay(options.SuspendTime);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    logger.LogInformation(ex.Message);
                                }
                            }
                        }));
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }


        /// <summary>
        /// 循环执行计划
        /// </summary>
        /// <param name="res"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private async Task<IntervalPanResponse> IntervalPan(string res, SubscribeIntervalAttribute attribute,ILogger<InitQCore> logger,ICacheService redis, InitQOptions options) 
        {
            try
            {
                var model = JsonConvert.DeserializeObject<IntervalMessage>(res);
                if (model == null) 
                {
                    if (logger != null && options.ShowLog) logger.LogWarning($"循环执行计划模型反序列化失败");
                    return new IntervalPanResponse() { Code = 0,Data = res,Message = $"循环执行计划模型反序列化失败" };
                }
                if (attribute == null) 
                {
                    if (logger != null && options.ShowLog) logger.LogWarning($"间隔消费消息配置异常");
                    return new IntervalPanResponse() { Code = 0, Data = res, Message = $"间隔消费消息配置异常" };
                }
                if(string.IsNullOrEmpty(attribute.IntervalList)) 
                {
                    if (logger != null && options.ShowLog) logger.LogWarning($"间隔数配置异常");
                    return new IntervalPanResponse() { Code = 0, Data = res, Message = $"间隔数配置异常" };
                }
                List<int> intervalList = new List<int>();
                try
                {
                    intervalList = attribute.IntervalList.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                }
                catch
                {
                    if (logger != null && options.ShowLog) logger.LogWarning($"间隔数配置类型异常");
                    return new IntervalPanResponse() { Code = 0, Data = res, Message = $"间隔数配置类型异常" };
                }
                //首次执行
                if(model.Num <= 0) 
                {
                    model.Num++;
                    return new IntervalPanResponse() { Code = 0, Data = JsonConvert.SerializeObject(model) };
                }

                var list = attribute.IntervalList.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                //最大执行次数
                attribute.MaxNum = attribute.MaxNum < 0 ? 0 : attribute.MaxNum;
                if (attribute.MaxNum != 0) 
                {
                    if(model.Num >= attribute.MaxNum) 
                    {
                        if (!string.IsNullOrEmpty(attribute.DeadLetterKey))
                        {
                            //丢人死信队列
                            return new IntervalPanResponse() { Code = -1, Data = res, Message = $"加入死信队列" };
                        }
                        return new IntervalPanResponse() { Code = -2, Data = res, Message = $"丢弃" };
                    }
                    
                }


                var intervalNum = 0;
                if (attribute.IntervalType == 0)
                {
                    intervalNum = model.Num > intervalList.Count ? intervalList[intervalList.Count - 1] : intervalList[model.Num - 1];
                }
                else
                {
                    intervalNum = intervalList[(model.Num-1) % intervalList.Count];
                }
                model.Num++;
                return new IntervalPanResponse() { Code = 1, Data = JsonConvert.SerializeObject(model), Message = intervalNum.ToString() };
            }
            catch (Exception ex)
            {
                if (logger != null && options.ShowLog) logger.LogWarning($"间隔消费消息计划异常,{ex.Message}|{ex.StackTrace}");
                return new IntervalPanResponse() { Code = 0, Data = res, Message = $"系统异常" };
            }
        }


        public async Task FindInterfaceTypes(IServiceProvider provider, InitQOptions options)
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
                List<Task> tasks = new List<Task>();
                //普通队列任务
                tasks.Add(Send(executorDescriptorList.Where(m => m.Attribute.GetType().Name == typeof(SubscribeAttribute).Name), provider, options));

                //延迟队列任务
                tasks.Add(SendDelay(executorDescriptorList.Where(m => m.Attribute.GetType().Name == typeof(SubscribeDelayAttribute).Name), provider, options));

                //间隔队列任务
                tasks.Add(SendInterval(executorDescriptorList.Where(m => m.Attribute.GetType().Name == typeof(SubscribeIntervalAttribute).Name), provider, options));
                await Task.WhenAll(tasks);
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
