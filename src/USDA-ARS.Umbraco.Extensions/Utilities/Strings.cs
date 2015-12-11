using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Utilities
{
    public class Strings
    {
        public static string UppercaseFirst(string words)
        {
            string output = null;

            if (false == string.IsNullOrEmpty(words))
            {
                output = char.ToUpper(words[0]) + words.Substring(1).ToLower();
            }

            return output;
        }
    }
}
