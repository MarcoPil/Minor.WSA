using System;

namespace Minor.WSA.Infrastructure
{
    public class RoutingKeyAttribute : Attribute
    {
        public string RoutingKey { get; }

        public RoutingKeyAttribute(string routingKey)
        {
            RoutingKey = routingKey;
        }
    }
}