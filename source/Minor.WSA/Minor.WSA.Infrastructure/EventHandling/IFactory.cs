using Microsoft.Extensions.DependencyInjection;

namespace Minor.WSA.Infrastructure
{
    public interface IFactory
    {
        object GetInstance();

    }
}