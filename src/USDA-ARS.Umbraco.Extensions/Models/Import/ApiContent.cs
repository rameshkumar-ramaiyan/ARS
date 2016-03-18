using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class ApiContent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string DocType { get; set; }
        public string Template { get; set; }
        public string Url { get; set; }
        public List<ApiProperty> Properties { get; set; }
        public int Save { get; set; }
        public List<ApiContent> ChildContentList { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
