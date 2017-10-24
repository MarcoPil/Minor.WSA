using Minor.WSA.Infrastructure.Test.SharedTests.Dummies;

internal class InjectingFactoryEventHandler
{
    public ISomethingToInject InjectedValue;

    public InjectingFactoryEventHandler(ISomethingToInject injection)
    {
        InjectedValue = injection;
    }
}