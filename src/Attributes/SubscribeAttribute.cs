using InitQ.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ.Attributes
{
    public class SubscribeAttribute: TopicAttribute
    {
        public SubscribeAttribute(string name) : base(name) 
        {

        }
    }
}
