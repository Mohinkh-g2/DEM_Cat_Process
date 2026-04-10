using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KeywordScoring.Data
{
    public static class RegexMatch
    {
        public static string RegexCapture(string searchtext, string pattern)
        {
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return r.Match(searchtext).Value.ToString();
        }
    }
}
