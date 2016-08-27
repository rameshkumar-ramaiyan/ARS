using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.Logging;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class ModeCodes
   {
      public static bool ValidateModeCode(string modeCode)
      {
         bool isValid = false;

         if (false == string.IsNullOrWhiteSpace(modeCode))
         {
            if (true == Regex.IsMatch(modeCode, @"\d{1,2}[\-]*\d{1,2}[\-]*\d{1,2}[\-]*\d{1,2}"))
            {
               isValid = true;
            }
         }

         return isValid;
      }


      public static string ModeCodeNoDashes(string modeCode)
      {
         if (true == ValidateModeCode(modeCode))
         {
            modeCode = modeCode.Replace("-", "").Replace("\\", "").Replace("/", "").Replace(" ", "");

            if (modeCode.Length == 8)
            {
               return modeCode;
            }
            else
            {
               return null;
            }
         }
         else
         {
            return null;
         }
      }

      public static string ModeCodeAddDashes(string modeCode)
      {
         if (true == ValidateModeCode(modeCode))
         {
            if (false == string.IsNullOrEmpty(modeCode) && modeCode.IndexOf("-") >= 0)
            {
               return modeCode;
            }
            else if (modeCode.Length == 8)
            {
               string modeCodeOut = modeCode.Substring(0, 2);

               modeCodeOut += "-" + modeCode.Substring(2, 2);
               modeCodeOut += "-" + modeCode.Substring(4, 2);
               modeCodeOut += "-" + modeCode.Substring(6, 2);

               return modeCodeOut;
            }
            else
            {
               return null;
            }
         }
         else
         {
            return null;
         }
      }


      public static string ModeCodeIntsToString(int modeCode1, int modeCode2, int modeCode3, int modeCode4)
      {
         string modeCode = "";

         if (modeCode1.ToString().Length == 1)
         {
            modeCode += "0" + modeCode1;
         }
         else
         {
            modeCode += modeCode1;
         }

         if (modeCode2.ToString().Length == 1)
         {
            modeCode += "-0" + modeCode2;
         }
         else
         {
            modeCode += "-" + modeCode2;
         }

         if (modeCode3.ToString().Length == 1)
         {
            modeCode += "-0" + modeCode3;
         }
         else
         {
            modeCode += "-" + modeCode3;
         }

         if (modeCode4.ToString().Length == 1)
         {
            modeCode += "-0" + modeCode4;
         }
         else
         {
            modeCode += "-" + modeCode4;
         }

         return modeCode;
      }


      public static List<string> ModeCodeArray(string modeCode)
      {
         if (true == ValidateModeCode(modeCode))
         {
            if (true == string.IsNullOrWhiteSpace(modeCode))
            {
               LogHelper.Warn(typeof(ModeCodes), "ModeCodeArray(): Mode Code is empty or invalid.");
            }

            modeCode = ModeCodeAddDashes(modeCode);

            return modeCode.Split('-').ToList();
         }
         else
         {
            return null;
         }
      }
   }
}
