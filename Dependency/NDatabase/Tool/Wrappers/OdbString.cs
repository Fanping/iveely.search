using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NDatabase.Tool.Wrappers
{
    internal static class OdbString
    {
        private static readonly Dictionary<string, Regex> Cache = new Dictionary<string, Regex>();

        internal static bool Matches(string regExp, string valueToCheck)
        {
            var regex = Cache.GetOrAdd(regExp, pattern => new Regex(pattern));

            return regex.IsMatch(valueToCheck);
        }

        /// <summary>
        ///   A small method for indentation
        /// </summary>
        /// <param name="currentDepth"></param>
        internal static string DepthToSpaces(int currentDepth)
        {
            var buffer = new StringBuilder();
            for (var i = 0; i < currentDepth; i++)
                buffer.Append("  ");
            return buffer.ToString();
        }
    }
}
