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
    public void BuilderFindsEventHandlerClasses()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(2, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.TestApp.EventHandlers.KlantbeheerEventHandler", result.Factories);
    }


    [Fact]
    public void BuilderRegistersQueueNames()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(2, result.EventListeners.Count());
        Assert.True(result.EventListeners.Any(listener => listener.QueueName == "MVM.Polisbeheer.KlantbeheerEvents"));
        Assert.True(result.EventListeners.Any(listener => listener.QueueName == "Unittest.WSA.Test"));

    }

    [Fact]
    public void BuilderFindsEventHandlingMethods()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        var handles = result.EventListeners.SelectMany(listener => listener.RoutingKeyExpressions);
        Assert.Equal(4, handles.Count());
        Assert.Contains("#.Klantbeheer.KlantGeregistreerd", handles);
        Assert.Contains("Test.WSA.KlantVerhuisd", handles);
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

}