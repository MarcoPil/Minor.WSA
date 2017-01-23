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

    //[Fact]
    //public void BuilderCreatesHost()
    //{
    //    var target = new MicroserviceHostBuilder();

    //    var result = target.UseConventions();

    //    Assert.Contains(result.ServiceModel.EventHandlers.Contains(handler => ));
    //}
}