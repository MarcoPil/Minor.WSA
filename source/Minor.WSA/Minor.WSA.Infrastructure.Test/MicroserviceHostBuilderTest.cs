using Minor.WSA.Infrastructure;
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

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.TestApp.EventHandlers.KlantbeheerEventHandler", result.Factories);
    }

    [Fact]
    public void BuilderFindsEventHandlingMethods()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(2, result.EventHandles.Count());
        Assert.Contains("#.Klantbeheer.KlantGeregistreerd", result.EventHandles);
        Assert.Contains("Test.WSA.KlantVerhuisd", result.EventHandles);
    }

}