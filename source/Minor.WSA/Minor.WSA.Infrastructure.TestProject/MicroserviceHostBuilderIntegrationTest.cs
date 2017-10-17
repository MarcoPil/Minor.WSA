using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class MicroserviceHostBuilderIntegrationTest
{
    [Fact]
    public void BuilderFindsEventHandlerClasses()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("MVM.Polisbeheer.EventHandlers.KlantbeheerEventListener", result.Factories);
    }


    [Fact]
    public void BuilderRegistersQueueNames()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        Assert.Equal(1, result.EventListeners.Count());
        Assert.Contains(result.EventListeners, listener => listener.QueueName == "MVM.Polisbeheer.KlantbeheerEvents");
    }

    [Fact]
    public void BuilderFindsEventHandlingMethods()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.UseConventions();

        var handles = result.EventListeners.SelectMany(listener => listener.TopicExpressions);
        Assert.Equal(2, handles.Count());
        Assert.Contains("#.Klantbeheer.KlantGeregistreerd", handles);
        Assert.Contains("MVM.Klantbeheer.KlantVerhuisd", handles);
    }
}