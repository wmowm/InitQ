using System;
using System.Collections.Generic;

namespace InitQ
{
    public class InitQOptions
    {
        /// <summary>
        /// redis连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 没消息时挂起时长(毫秒)
        /// </summary>
        public int SuspendTime { get; set; }

        /// <summary>
        /// 每次消费消息间隔时间(毫秒)
        /// </summary>
        public int IntervalTime { get; set; }

        /// <summary>
        /// 是否显示日志
        /// </summary>
        [Obsolete("现在使用ILogger控制日志输出，不再需要此选项")]
        public bool ShowLog { get; set; }

        /// <summary>
        /// 需要注入的类型
        /// </summary>
        public IList<Type> ListSubscribe { get; set; }

        public InitQOptions()
        {
            ConnectionString = "";
            IntervalTime = 0;
            SuspendTime = 1000;
        }
    }
}
