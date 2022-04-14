using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InitQ.Cache
{
    public interface ICacheService
    {

        ConnectionMultiplexer GetRedis();
        IDatabase GetDatabase();

        /// <summary>
        /// 缓存过期时间
        /// </summary>
        int TimeOut { set; get; }
        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        string Get(string key);

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        Task<string> GetAsync(string key);
        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        bool Remove(string key);

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        //void Clear();

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        bool Set(string key, object data);

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
         Task<bool> SetAsync(string key, object data);


        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间秒</param>
        bool Set(string key, object data, int cacheTime);


        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        bool Set(string key, object data, TimeSpan cacheTime);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间秒</param>
        Task<bool> SetAsync(string key, object data, int cacheTime);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间秒</param>
        Task<bool> SetAsync(string key, object data, TimeSpan cacheTime);
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 模糊查询key的集合
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string[] ScriptEvaluateKeys(string key);

        /// <summary>
        /// 入列
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<long> ListLeftPushAsync(string key, string value);

        long ListLeftPush(string key, string value);

        Task<long> ListRightPushAsync(string key, string value);

        long ListRightPush(string key, string value);

        /// <summary>
        /// 出列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> ListLeftPopAsync<T>(string key) where T : class;

        T ListLeftPop<T>(string key) where T : class;


        Task<T> ListRightPopAsync<T>(string key) where T : class;

        T ListRightPop<T>(string key) where T : class;

        Task<string> ListLeftPopAsync(string key);

        string ListLeftPop(string key);

        Task<string> ListRightPopAsync(string key);

        string ListRightPop(string key);

        /// <summary>
        /// 获取队列长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> ListLengthAsync(string key);

        long ListLength(string key);

        /// <summary>
        /// 通道广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        long Publish(string key, string msg);

        /// <summary>
        /// 通道广播
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task<long> PublishAsync(string key, string msg);

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        void Subscribe(string key, Action<RedisChannel, RedisValue> action);

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task SubscribeAsync(string key, Action<RedisChannel, RedisValue> action);

        /// <summary>
        /// 插入zset
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="msg">消息</param>
        /// <param name="score">序号</param>
        /// <returns></returns>
        Task<bool> SortedSetAddAsync(string key, string msg, double score);


        /// <summary>
        /// 插入zset
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="msg">消息</param>
        /// <param name="time">延迟执行时间</param>
        /// <returns></returns>
        Task<bool> SortedSetAddAsync(string key, string msg, DateTime time);

        /// <summary>
        /// 查询zset
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="start">序号开始</param>
        /// <param name="stop">序号结束</param>
        /// <param name="exclude"></param>
        /// <param name="order">排序</param>
        /// <returns></returns>
        Task<string[]> SortedSetRangeByScoreAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending);


        /// <summary>
        /// 查询zset
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="startTime">延迟执行时间开始</param>
        /// <param name="stopTime">延迟执行时间结束</param>
        /// <param name="exclude"></param>
        /// <param name="order">排序</param>
        /// <returns></returns>
        Task<string[]> SortedSetRangeByScoreAsync(string key, DateTime? startTime, DateTime? stopTime, Exclude exclude = Exclude.None, Order order = Order.Ascending);


        /// <summary>
        /// 删除zset元素
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="start">序号开始</param>
        /// <param name="stop">序号结束</param>
        /// <returns></returns>
        Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop);

        /// <summary>
        /// 删除zset元素
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="start">延迟执行时间开始</param>
        /// <param name="stop">延迟执行时间结束</param>
        /// <returns></returns>
        Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, DateTime? startTime, DateTime? stopTime);

        /// <summary>
        /// 计数器(递增)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cacheTime">过期时间</param>
        /// <param name="value">步长</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        long Increment(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// 计数器(递增)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cacheTime">过期时间</param>
        /// <param name="value">步长</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        Task<long> IncrementAsync(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// 计数器(递减)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cacheTime">过期时间</param>
        /// <param name="value">步长</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        long Decrement(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// 计数器(递减)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cacheTime">过期时间</param>
        /// <param name="value">步长</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        Task<long> DecrementAsync(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None);
    }
}
