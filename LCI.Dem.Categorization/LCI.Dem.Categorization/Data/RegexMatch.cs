using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data
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
