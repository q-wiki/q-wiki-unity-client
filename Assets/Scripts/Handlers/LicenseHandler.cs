using System;
using System.Text.RegularExpressions;

namespace Handlers
{
    public static class LicenseHandler
    {

        private static readonly string LINK_REGEX = "\"([\\s\\S]*)\"";
        private static readonly string VALUE_REGEX = ">([\\s\\S]*)</link>";

        public static Tuple<string, string> GetLinkAndValue(string str)
        {

            var linkMatches = Regex.Matches(str, LINK_REGEX);
            var valueMatches = Regex.Matches(str, VALUE_REGEX);

            string link = linkMatches.Count < 1 ? null : linkMatches[0].Groups[1].Value;

            string value = valueMatches.Count < 1 ? null : valueMatches[0].Groups[1].Value;

            return new Tuple<string, string>(link, value);

        }
    }
}