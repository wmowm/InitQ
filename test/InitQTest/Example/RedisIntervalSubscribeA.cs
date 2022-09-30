using InitQ.Abstractions;
using InitQ.Attributes;
using InitQ.Cache;
using InitQ.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQTest.Example
{
    public class RedisIntervalSubscribeA : IRedisSubscribe
    {
        private readonly ICacheService _redisService;
        public RedisIntervalSubscribeA(ICacheService redisService) 
        {
            _redisService = redisService;
        }


        [SubscribeInterval("tibos_interval_test_1",0,"2,3,5,10",1,"dead_tibos_test_1")]
        private async Task SubscribeIntervalTest(string msg)
        {
            Console.WriteLine($"A类间隔执行--->当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅者A消费消息:{msg}");
            await _redisService.ListLeftPushAsync<IntervalMessage>("tibos_interval_test_1",JsonConvert.DeserializeObject<IntervalMessage>(msg));
        }
    }
}
