Redis消息队列中间件
===============

## 安装环境

>+ .net core版本：2.1
>+ redis版本：3.0以上

### 特点
~~~
1.通过注解的方式,订阅队列
2.可以设置消费消息的频次
3.支持消息广播
4.1.0.0.7版本新增延迟队列支持
~~~

### 应用场景
分布式环境,redis消息队列

### 使用介绍
+ 1.获取initQ包 </br>
    方案A. install-package InitQ
    方案B. nuget包管理工具搜索 InitQ

+ 2.添加中间件(该中间件依赖StackExchange.Redis)
    ```code
    services.AddInitQ(m=> 
    {
        m.SuspendTime = 1000;
        m.ConnectionString = "47.104.247.70,password=admin";
        m.ListSubscribe = new List<Type>() { typeof(RedisSubscribeA), typeof(RedisSubscribeB) };
        m.ShowLog = false;
    });
    ```
### 消息发布/订阅
+ 1.定义发布者
  ```code
    using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        //redis对象
        var _redis = scope.ServiceProvider.GetService<ICacheService>();
        //循环向 tibos_test_1 队列发送消息
        for (int i = 0; i < 1000; i++)
        {
            await _redis.ListRightPushAsync("tibos_test_1", $"我是消息{i + 1}号");
        }
    }
  ```
+ 2.定义消费者类 RedisSubscribeA
    ```code
    public class RedisSubscribeA: IRedisSubscribe
    {
        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest(string msg)
        {
            Console.WriteLine($"A类--->订阅者A消息消息:{msg}");
        }

        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest1(string msg)
        {
            Console.WriteLine($"A类--->订阅者A1消息消息:{msg}");
        }

        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest2(string msg)
        {
            Console.WriteLine($"A类--->订阅者A2消息消息:{msg}");
        }

        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest3(string msg)
        {
            Console.WriteLine($"A类--->订阅者A3消息消息:{msg}");
        }
    }
    ```
+ 3.定义消费者类 RedisSubscribeB
    ```code
    public class RedisSubscribeB : IRedisSubscribe
    {
        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest(string msg)
        {
            Console.WriteLine($"B类--->订阅者B消费消息:{msg}");
        }
    }
    ```
### 消息广播/订阅
+ 1.订阅消息通道,订阅者需要在程序初始化的时候启动一个线程侦听通道,这里使用HostedService来实现,并注册到容器
  ```code
    public class ChannelSubscribeA : IHostedService, IDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public ChannelSubscribeA(ILogger<TestMain> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }
        public void Dispose()
        {
            _logger.LogInformation("退出");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("程序启动");
            Task.Run(async () =>
            {
                using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //redis对象
                    var _redis = scope.ServiceProvider.GetService<ICacheService>();
                    await _redis.SubscribeAsync("test_channel", new Action<RedisChannel, RedisValue>((channel, message) =>
                    {
                        Console.WriteLine("test_channel" + " 订阅服务A收到消息：" + message);
                    }));

                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("结束");
            return Task.CompletedTask;
        }
    }
  ```
  ```code
    public class ChannelSubscribeB : IHostedService, IDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public ChannelSubscribeB(ILogger<TestMain> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }
        public void Dispose()
        {
            _logger.LogInformation("退出");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("程序启动");
            Task.Run(async () =>
            {
                using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //redis对象
                    var _redis = scope.ServiceProvider.GetService<ICacheService>();
                    await _redis.SubscribeAsync("test_channel", new Action<RedisChannel, RedisValue>((channel, message) =>
                    {
                        Console.WriteLine("test_channel" + " 订阅服务B收到消息：" + message);
                    }));

                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("结束");
            return Task.CompletedTask;
        }
    }
  ```
+ 2.将HostedService类注入到容器
  ```code
    services.AddHostedService<ChannelSubscribeA>();
    services.AddHostedService<ChannelSubscribeB>();
  ```
+ 3.广播消息
  ```code
    using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        //redis对象
        var _redis = scope.ServiceProvider.GetService<ICacheService>();
        for (int i = 0; i < 1000; i++)
        {
            await _redis.PublishAsync("test_channel", $"往通道发送第{i}条消息");
        }
    }
  ```
### 使用zset实现延迟队列(>=1.0.0.7)
+ 1.定义发布者
  ```
    Task.Run(async () =>
    {

        using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            //redis对象
            var _redis = scope.ServiceProvider.GetService<ICacheService>();

            for (int i = 0; i < 100; i++)
            {
                var dt = DateTime.Now.AddSeconds(3 * (i + 1));
                //key:redis里的key,唯一
                //msg:任务
                //time:延时执行的时间
                await _redis.SortedSetAddAsync("test_0625", $"延迟任务,第{i + 1}个元素,执行时间:{dt.ToString("yyyy-MM-dd HH:mm:ss")}", dt);
            }
        }
    });
  ```
+ 2.定义消费者
  ```
    //延迟队列
    [SubscribeDelay("test_0625")]
    private async Task SubRedisTest1(string msg)
    {
        Console.WriteLine($"A类--->当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅者延迟队列消息开始--->{msg}");
        //模拟任务执行耗时
        var m = new Random().Next(1,10);
        await Task.Delay(TimeSpan.FromSeconds(m));
        Console.WriteLine($"A类--->{msg} 结束<---");
    }
  ```
  
### 版本
+ V1.0       更新时间:2019-12-30

### 版本库：
+ Git获取：https://github.com/wmowm/InitQ


### 作者：提伯斯
