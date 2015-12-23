using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class Content
    {
        public string ApiKey { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string DocType { get; set; }
        public string Template { get; set; }
        public List<Property> Properties { get; set; }
        public int Save { get; set; }
    }
}
