using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportPeopleSites.Objects
{
    public class Logs
    {
        public static void AddLog(ref string LogFileText, string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LogFileText += line + "\r\n";
        }
    }
}
