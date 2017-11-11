using Minor.WSA.Common;
using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// Implementations for this interface should publish Domain Events on the Event Bus.
    /// </summary>
    public interface IEventPublisher : IDisposable
    {
        void Publish(DomainEvent domainEvent);
    }
}
