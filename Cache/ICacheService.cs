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
        T Get<T>(string key);
        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);
        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        //void Clear();
        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        void Set(string key, object data);
        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间秒</param>
        void Set(string key, object data, int cacheTime);
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        bool Exists(string key);



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
    }
}
