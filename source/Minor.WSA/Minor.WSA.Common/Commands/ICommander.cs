using System.Threading.Tasks;
using Minor.WSA.Common;
using System;

namespace Minor.WSA.Infrastructure
{
    public interface ICommander : IDisposable
    {
        Task<T> ExecuteAsync<T>(string serviceName, DomainCommand command);
    }
}