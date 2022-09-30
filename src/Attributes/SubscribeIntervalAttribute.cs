using InitQ.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ.Attributes
{

    public class SubscribeIntervalAttribute : TopicAttribute
    {

        /// <summary>
        /// 最大执行次数,0不设置,超过后丢入死信队列,无死信队列则丢弃
        /// </summary>
        public int MaxNum { get; set; }

        /// <summary>
        /// 间隔类型
        /// 0.根据执行次数取值(间隔时间递增)  --> 2,3,5,10,10,10,10
        /// 1.根据执行次数取模(间隔时间周期)  --> 2,3,5,10,2,3,5,10
        /// </summary>
        public int IntervalType { get; set; }

        /// <summary>
        /// 间隔数,单位秒,分隔(2,3,5,10)
        /// </summary>
        public string IntervalList { get; set; }

        /// <summary>
        /// 死信队列key
        /// </summary>
        public string DeadLetterKey { get; set; }

        /// <summary>
        /// 重复执行队列
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="intervalList">间隔数</param>
        /// <param name="maxNum">最大执行次数,超过后丢入死信队列,无死信队列则丢弃</param>
        /// <param name="intervalType">间隔类型</param>
        /// <param name="deadLetterKey">死信队列key</param>
        public SubscribeIntervalAttribute(string name, int maxNum=0, string intervalList="",int intervalType=1,string deadLetterKey="") : base(name)
        {
            MaxNum = maxNum;
            IntervalType = intervalType;
            IntervalList = intervalList;
            DeadLetterKey = deadLetterKey;
        }
    }
}
