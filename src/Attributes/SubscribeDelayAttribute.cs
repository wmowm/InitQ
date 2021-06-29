using InitQ.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ.Attributes
{

    public class SubscribeDelayAttribute : TopicAttribute
    {
        public SubscribeDelayAttribute(string name) : base(name)
        {

        }
    }
}
