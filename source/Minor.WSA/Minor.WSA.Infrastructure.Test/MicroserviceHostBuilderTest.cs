using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class MicroserviceHostBuilderTest
{
    [Fact]
    public void BuilderCreatesHost()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.CreateHost();

        Assert.NotNull(result);
    }

    [Fact]
    public void BuilderAddEventHandler()
    {
        var target = new MicroserviceHostBuilder();

        MicroserviceHostBuilder result = target.AddEventHandler<AnotherEventHandler>();

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.AnotherEventHandler", result.Factories);
        var routingkeys = result.EventListeners.First().RoutingKeyExpressions;
        Assert.Equal(2, routingkeys.Count());
        Assert.Contains("#.Another.SomeEvent", routingkeys);
        Assert.Contains("WSA.Test.OtherEvent", routingkeys);
    }


    [Fact]
    public void IncorrectRoutingkey()
    {
        var target = new MicroserviceHostBuilder();

        Action action = () =>
        {
            MicroserviceHostBuilder result = target.AddEventHandler<IncorrectRoutingKeyEventHandler>();
        };

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        Assert.Equal("Routingkey Expression '#OtherEvent' has an invalid expression format.", ex.Message);
    }

}