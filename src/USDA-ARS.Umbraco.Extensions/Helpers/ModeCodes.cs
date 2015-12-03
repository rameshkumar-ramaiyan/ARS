using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class ModeCodes
    {
        public static string ModeCodeNoDashes(string modeCode)
        {
            return modeCode.Replace("-", "");
        }


        public static string ModeCodeAddDashes(string modeCode)
        {
            if (modeCode.IndexOf("-") >= 0)
            {
                return modeCode;
            }
            else if (modeCode.Length == 8)
            {
                string modeCodeOut = modeCode.Substring(0, 2);

                modeCodeOut += modeCode.Substring(2, 2);
                modeCodeOut += modeCode.Substring(4, 2);
                modeCodeOut += modeCode.Substring(6, 2);

                return modeCodeOut;
            }
            else
            {
                return null;
            }
        }


        public static List<string> ModeCodeArray(string modeCode)
        {
            modeCode = ModeCodeAddDashes(modeCode);

            return modeCode.Split('-').ToList();
        }
    }
}
