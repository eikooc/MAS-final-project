using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Classes
{
    public static class Utility
    {
        public static bool Matches(this string str, string pattern)
        {
            Regex regex = new Regex(pattern);
            return regex.Match(str).Success;            
        }
    }
}
