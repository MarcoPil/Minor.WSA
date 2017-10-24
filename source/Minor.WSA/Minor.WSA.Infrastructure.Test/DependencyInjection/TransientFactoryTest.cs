using Microsoft.Extensions.DependencyInjection;
using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test.SharedTests.Dummies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class TransientFactoryTest
{
    [Fact]
    public void TransientFactory_ProducesCorrectObject()
    {
        IFactory target = new TransientFactory(new ServiceCollection(), typeof(FactoryEventHandler));

        var result = target.GetInstance();

        Assert.IsType<FactoryEventHandler>(result);
    }

    [Fact]
    public void TransientFactory_DoesDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddTransient<ISomethingToInject, SomethingToInject>();
        IFactory target = new TransientFactory(services, typeof(InjectingFactoryEventHandler));

        var result = target.GetInstance();

        Assert.IsType<InjectingFactoryEventHandler>(result);
        Assert.Equal("injection succeeded", (result as InjectingFactoryEventHandler).InjectedValue.InjectionValue());
    }

    [Fact]
    public void TransientFactory_ProducesTransientObjects()
    {
        var services = new ServiceCollection();
        IFactory target = new TransientFactory(services, typeof(FactoryEventHandler));

        var result1 = target.GetInstance();
        var result2 = target.GetInstance();

        Assert.NotStrictEqual(result1, result2);
    }

}