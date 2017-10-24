using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// Utility class for matching routingkeys to topic expressions.
    /// </summary>
    public static class RoutingKeyMatcher
    {
        public static IEnumerable<string> ThatMatch(this IEnumerable<string> topicExpressions, string routingKey)
        {
            return topicExpressions.Where(expr => IsMatch(expr, routingKey));
        }

        public static bool IsMatch(string topicExpression, string routingKey)
        {
            var pattern = topicExpression
                      .Replace(@".", @"\.")
                      .Replace(@"*", @"[^.]*")
                      .Replace(@"#", @".*")
                      ;
            pattern = "^" + pattern + "$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(routingKey);
        }

        private const string part = @"([^.#*]+|\#|\*)";
        private const string TopicPattern = @"^" + part + @"(\." + part + @")*$";
        private static readonly Regex ValidTopicRegex = new Regex(TopicPattern, RegexOptions.Compiled);

        public static bool IsValidTopicExpression(string expression)
        {
            return ValidTopicRegex.IsMatch(expression);
        }
    }
}