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


### 作者：提伯斯
