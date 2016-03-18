using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public class InterLinksView
    {
        public string Type { get; set; }
        public long Id { get; set; }
        public string Description { get; set; }
    }
}
