using Minor.WSA.Infrastructure.Test.TestApp.Commands;

namespace Minor.WSA.Infrastructure.Test.TestApp
{
    public interface IPolisService
    {
        void RegistreerPolis(RegisteerPolisCommand command);
    }
}