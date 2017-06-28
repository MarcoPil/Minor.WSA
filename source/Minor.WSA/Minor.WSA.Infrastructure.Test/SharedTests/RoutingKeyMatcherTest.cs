using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using Minor.WSA.Infrastructure.Test.EventHandlerTests;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RabbitMQ.Client.Events;

public class RoutingKeyMatcherTest
{
    [Fact]
    public void IsMatchTest()
    {
        Assert.True(RoutingKeyMatcher.IsMatch("MVM.Test.Match", "MVM.Test.Match"), "MVM.Test.Match == MVM.Test.Match");
        Assert.False(RoutingKeyMatcher.IsMatch("MVM.Test.NoMatch", "MVM.Test.Match"), "MVM.Test.NoMatch  !=  MVM.Test.Match");

        Assert.True(RoutingKeyMatcher.IsMatch("MVM.Test.*", "MVM.Test.Match"), "MVM.Test.* == MVM.Test.Match");
        Assert.True(RoutingKeyMatcher.IsMatch("MVM.*.Match", "MVM.Test.Match"), "MVM.*.Match == MVM.Test.Match");
        Assert.True(RoutingKeyMatcher.IsMatch("*.Test.Match", "MVM.Test.Match"), "*.Test.Match == MVM.Test.Match");
        Assert.False(RoutingKeyMatcher.IsMatch("*.Match", "MVM.Test.Match"), "*.Match  !=  MVM.Test.Match");
        Assert.False(RoutingKeyMatcher.IsMatch("MVM.*.Match", "MVM.Test.To.Match"), "MVM.*.Match  !=  MVM.Test.To.Match");

        Assert.True(RoutingKeyMatcher.IsMatch("#.Match", "MVM.Test.Match"), "#.Match  ==  MVM.Test.Match");
        Assert.True(RoutingKeyMatcher.IsMatch("#", "MVM.Test.Match"), "#  ==  MVM.Test.Match");
    }

    [Fact]
    public void IsValidRoutingKeyExpressionTest()
    {
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("Test"), "Test");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("*"), "*");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("#"), "#");

        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.Test"), "MVM.Test");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.*"), "MVM.*");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.#"), "MVM.#");

        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("Test.Event"), "Test.Event");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("*.Event"), "*.Event");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("#.Event"), "#.Event");

        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.Test.Event"), "MVM.Test.Event");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.*.Event"), "MVM.*.Event");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.#.Event"), "MVM.#.Event");

        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("Test.Event.#"), "Test.Event.#");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("*.Event.#"), "*.Event.#");
        Assert.True(RoutingKeyMatcher.IsValidRoutingKeyExpression("*.*.#.Event.#"), "*.*.#.Event.#");

        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("#Event"), "#Event");
        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("*Event"), "*Event");
        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("*#"), "*#");
        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.#Event"), "MVM.#Event");
        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.*Event"), "MVM.*Event");
        Assert.False(RoutingKeyMatcher.IsValidRoutingKeyExpression("MVM.*#"), "MVM.*#");

    }
}