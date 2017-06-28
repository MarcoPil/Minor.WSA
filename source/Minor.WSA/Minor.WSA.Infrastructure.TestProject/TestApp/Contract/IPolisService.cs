using MVM.Polisbeheer.Commands;

namespace MVM.Polisbeheer
{
    public interface IPolisService
    {
        void RegistreerPolis(RegisteerPolisCommand command);
    }
}