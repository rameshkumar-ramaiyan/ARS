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


        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }
    }
}
