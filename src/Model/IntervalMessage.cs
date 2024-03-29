﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ.Model
{
    /// <summary>
    /// 间隔消费消息
    /// </summary>
    public class IntervalMessage
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 当前执行次数
        /// </summary>
        public int Num { get; set; }
    }
}
