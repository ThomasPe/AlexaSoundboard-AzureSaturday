using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexaSoundboard.Utils
{
    public static class StringExtensions
    {
        public static string AsFileName(this string str)
        {
            return str.ToLower().Replace(" ", "");
        }
    }
}
