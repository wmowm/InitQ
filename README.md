Redis消息队列中间件
===============

## 安装环境

>+ .net core版本：2.1
>+ redis版本：3.0以上



### 特点
~~~
1.很方便的使用redis消息队列,开箱即用
2.可以设置消费消息的频次

~~~

### 应用场景
分布式环境,redis消息队列

### 使用介绍
+ 1.获取initQ包
    install-package InitQ
+ 2.添加中间件
    ```code
    services.AddInitQ(m=> 
    {
        m.SuspendTime = 1000;
        m.ConnectionString = "47.104.247.70,password=admin";
        m.ListSubscribe = new List<Type>() { typeof(RedisSubscribe2), typeof(RedisSubscribe3) };
        m.ShowLog = false;
    });
    ```
+ 3.定义消费者类
    ```code
    public class RedisSubscribe : IRedisSubscribe 
    {
        [Subscribe("bbb")]
        private async Task SubRedisOrderMessage(string order)
        {
            Console.WriteLine($"队列bbb消费消息:{order}");
        }
    }
    ```

### 版本
+ V1.0       更新时间:2019-12-30

### 版本库：
+ Git获取：https://github.com/wmowm/InitQ


### 作者：提伯斯

