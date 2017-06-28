using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// Utility class for routingkey expressions.
    /// </summary>
    public class RoutingKeyMatcher
    {

        public static string Match(string routingKey, IEnumerable<string> routingKeyExpressions)
        {
            return routingKeyExpressions.First(expr => IsMatch(expr, routingKey));
        }

        public static bool IsMatch(string routingKeyExpression, string routingKey)
        {
            var pattern = routingKeyExpression
                      .Replace(@".", @"\.")
                      .Replace(@"*", @"[^.]*")
                      .Replace(@"#", @".*")
                      ;
            pattern = "^" + pattern + "$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(routingKey);
        }

        private const string part = @"([^.#*]*|\#|\*)";
        private const string RoutingKeyPattern = @"^" + part + @"(\." + part + @")*$";
        private static readonly Regex ValidRoutingKeyRegex = new Regex(RoutingKeyPattern, RegexOptions.Compiled);

        public static bool IsValidRoutingKeyExpression(string expression)
        {
            return ValidRoutingKeyRegex.IsMatch(expression);
        }
    }
}