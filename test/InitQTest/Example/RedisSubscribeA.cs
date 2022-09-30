using InitQ.Abstractions;
using InitQ.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQTest.Example
{
    public class RedisSubscribeA : IRedisSubscribe
    {
        [Subscribe("tibos_test_1")]
        private async Task SubRedisTest(string msg)
        {
            Console.WriteLine($"A类--->当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅者A消费消息:{msg}");
            Thread.Sleep(3000); //使用堵塞线程模式,同步延时
            Console.WriteLine($"A类<---当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅者A消费消息:{msg} 完成");
        }


        [SubscribeDelay("test_dalay_test_1")]
        private async Task SubRedisTest1(string msg)
        {
            Console.WriteLine($"A类--->当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅者延迟队列消息开始--->{msg}");
            //模拟任务执行耗时
            await Task.Delay(TimeSpan.FromSeconds(3));
            Console.WriteLine($"A类--->{msg} 结束<---");
        }
    }
}
