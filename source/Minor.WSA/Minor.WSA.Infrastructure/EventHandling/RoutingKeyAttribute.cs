using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// This attribute should decorate each eventhandling method.
    /// All events matching the RoutingKeyExpression will be handled by this method. (the event wil possibly also handled by other methods with a matching routingKeyExpression
    /// </summary>
    public class RoutingKeyAttribute : Attribute
    {
        public string RoutingKey { get; }

        public RoutingKeyAttribute(string routingKeyExpression)
        {
            RoutingKey = routingKeyExpression;
        }
    }
}