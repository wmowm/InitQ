using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ.Model
{
    public class IntervalPanResponse
    {
        /// <summary>
        /// 状态码(0:正常推送,-1:加入死信队列)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 日志说明
        /// </summary>
        public string Message { get; set; }
    }
}
