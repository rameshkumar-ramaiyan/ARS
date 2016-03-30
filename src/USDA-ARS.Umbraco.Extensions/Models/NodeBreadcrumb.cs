using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public class NodeBreadcrumb
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Level { get; set; }

        public NodeBreadcrumb()
        {

        }

        public NodeBreadcrumb(string name, string url, int level)
        {
            Name = name;
            Url = url;
            Level = level;
        }
    }
}
