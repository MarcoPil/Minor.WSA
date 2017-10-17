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
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test"), "Test");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*"), "*");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("#"), "#");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.Test"), "MVM.Test");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.*"), "MVM.*");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.#"), "MVM.#");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test.Event"), "Test.Event");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.Event"), "*.Event");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("#.Event"), "#.Event");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.Test.Event"), "MVM.Test.Event");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.*.Event"), "MVM.*.Event");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.#.Event"), "MVM.#.Event");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test.Event.#"), "Test.Event.#");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.Event.#"), "*.Event.#");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.*.#.Event.#"), "*.*.#.Event.#");

        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("#Event"), "#Event");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("*Event"), "*Event");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("*#"), "*#");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.#Event"), "MVM.#Event");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.*Event"), "MVM.*Event");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.*#"), "MVM.*#");
    }

    [Fact]
    public void MatchShouldFindMultipleRoutingKeyExpressions()
    {
        // Arrange
        string[] routingKeyExpressions = { "MVM.*.Event", "*.Test.Event" };
        string routingKey = "MVM.Test.Event";

        // Act
        IEnumerable<string> matchingExpressions = RoutingKeyMatcher.ThatMatch(routingKeyExpressions, routingKey);

        // Assert
        Assert.Contains("MVM.*.Event", matchingExpressions);
        Assert.Contains("*.Test.Event", matchingExpressions);
    }
}