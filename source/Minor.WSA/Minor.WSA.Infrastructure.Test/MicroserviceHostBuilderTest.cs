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
    public void BuilderFindsEventHandlingMethods()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(4, result.EventHandles.Count());
        Assert.Contains("#.Klantbeheer.KlantGeregistreerd", result.EventHandles);
        Assert.Contains("Test.WSA.KlantVerhuisd", result.EventHandles);
    }

    [Fact]
    public void BuilderAddEventHandler()
    {
        var target = new MicroserviceHostBuilder();

        MicroserviceHostBuilder result = target.AddEventHandler<AnotherEventHandler>();

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.AnotherEventHandler", result.Factories);
        Assert.Equal(2, result.EventHandles.Count());
        Assert.Contains("#.Another.SomeEvent", result.EventHandles);
        Assert.Contains("WSA.Test.OtherEvent", result.EventHandles);
    }

}