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
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test"), "'Test' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*"), "'*' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("#"), "'#' should be a valid expression.");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.Test"), "'MVM.Test' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.*"), "'MVM.*' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.#"), "'MVM.#' should be a valid expression.");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test.Event"), "'Test.Event' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.Event"), "'*.Event' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("#.Event"), "'#.Event' should be a valid expression.");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.Test.Event"), "'MVM.Test.Event' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.*.Event"), "'MVM.*.Event' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("MVM.#.Event"), "'MVM.#.Event' should be a valid expression.");

        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("Test.Event.#"), "'Test.Event.#' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.Event.#"), "'*.Event.#' should be a valid expression.");
        Assert.True(RoutingKeyMatcher.IsValidTopicExpression("*.*.#.Event.#"), "'*.*.#.Event.#' should be a valid expression.");

        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("#Event"), "'#Event' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("*Event"), "'*Event' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("*#"), "'*#' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.#Event"), "'MVM.#Event' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.*Event"), "'MVM.*Event' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression("MVM.*#"), "'MVM.*#' should NOT be a valid expression.");
        Assert.False(RoutingKeyMatcher.IsValidTopicExpression(""), "[empty string] should NOT be a valid expression.");
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