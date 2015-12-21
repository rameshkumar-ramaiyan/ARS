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


        public static string FormatPhoneNumber(string phone)
        {
            string output = null;

            if (false == string.IsNullOrEmpty(phone) && phone.Length == 10)
            {
                output = "(" + phone.Substring(0, 3) + ") " + phone.Substring(3, 3) + "-" + phone.Substring(6, 4);
            }
            else
            {
                output = phone;
            }

            return output;
        }


        public static string CleanSqlString(string str)
        {
            return str.Replace("'", "''").Replace(";", "");
        }
    }
}
