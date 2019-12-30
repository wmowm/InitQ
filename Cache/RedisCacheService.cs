using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQ.Cache
{
    public class RedisCacheService : ICacheService
    {
        int Default_Timeout = 60 * 10 * 10;//默认超时时间（单位秒）
        JsonSerializerSettings jsonConfig = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        ConnectionMultiplexer connectionMultiplexer;
        IDatabase database;

        class CacheObject<T>
        {
            public int ExpireTime { get; set; }
            public bool ForceOutofDate { get; set; }
            public T Value { get; set; }
        }

        public RedisCacheService(ConfigurationOptions options)
        {
            //设置线程池最小连接数
            ThreadPool.SetMinThreads(200, 200);

            connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            database = connectionMultiplexer.GetDatabase();
        }


        public RedisCacheService(string configuration)
        {
            //设置线程池最小连接数
            ThreadPool.SetMinThreads(200, 200);

            connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            database = connectionMultiplexer.GetDatabase();
        }

        public ConnectionMultiplexer GetRedis()
        {
            return connectionMultiplexer;
        }

        public IDatabase GetDatabase()
        {
            return database;
        }

        /// <summary>
        /// 连接超时设置
        /// </summary>
        public int TimeOut
        {
            get
            {
                return Default_Timeout;
            }
            set
            {
                Default_Timeout = value;
            }
        }

        public string Get(string key)
        {
            return database.StringGet(key);
        }

        public T Get<T>(string key)
        {
            var cacheValue = database.StringGet(key);
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }

        public void Set(string key, object data)
        {
            database.StringSet(key, JsonConvert.SerializeObject(data));
        }

        public void Set(string key, object data, int cacheTime)
        {
            var timeSpan = TimeSpan.FromSeconds(cacheTime);
            database.StringSet(key, JsonConvert.SerializeObject(data), timeSpan);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            database.KeyDelete(key, CommandFlags.HighPriority);
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public bool Exists(string key)
        {
            return database.KeyExists(key);
        }

        /// <summary>
        /// 模糊查询key的集合
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] ScriptEvaluateKeys(string key)
        {
            var redisResult = database.ScriptEvaluate(LuaScript.Prepare(
                //Redis的keys模糊查询：
                " local res = redis.call('KEYS', @keypattern) " +
                " return res "), new { @keypattern = key });

            string[] preSult = (string[])redisResult;//将返回的结果集转为数组
            return preSult;
        }

        public long ListLeftPush(string key, string value)
        {
            var res = database.ListLeftPush(key, value);
            return res;
        }
        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            var res = await database.ListLeftPushAsync(key, value);
            return res;
        }

        public long ListRightPush(string key, string value)
        {
            var res = database.ListRightPush(key, value);
            return res;
        }
        public async Task<long> ListRightPushAsync(string key, string value)
        {
            var res = await database.ListRightPushAsync(key, value);
            return res;
        }

        public T ListLeftPop<T>(string key) where T : class
        {
            var cacheValue = database.ListLeftPop(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }
        public async Task<T> ListLeftPopAsync<T>(string key) where T : class
        {
            var cacheValue = await database.ListLeftPopAsync(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }


        public T ListRightPop<T>(string key) where T : class
        {
            var cacheValue = database.ListRightPop(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }
        public async Task<T> ListRightPopAsync<T>(string key) where T : class
        {
            var cacheValue = await database.ListRightPopAsync(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }


        public string ListLeftPop(string key)
        {
            var cacheValue = database.ListLeftPop(key);
            return cacheValue;
        }
        public async Task<string> ListLeftPopAsync(string key)
        {
            var cacheValue = await database.ListLeftPopAsync(key);
            return cacheValue;
        }


        public string ListRightPop(string key)
        {
            var cacheValue = database.ListRightPop(key);
            return cacheValue;
        }
        public async Task<string> ListRightPopAsync(string key)
        {
            var cacheValue = await database.ListRightPopAsync(key);
            return cacheValue;
        }



        public long ListLength(string key)
        {
            return database.ListLength(key);
        }
        public async Task<long> ListLengthAsync(string key)
        {
            return await database.ListLengthAsync(key);
        }
    }
}
