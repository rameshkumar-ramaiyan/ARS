using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportDocs.Models
{
    public class ImportPage
    {
        public string Title { get; set; }
        public string BodyText { get; set; }
        public string OldDocType { get; set; }
        public int OldDocId { get; set; }
        public int PageNumber { get; set; }
        public bool DisableTitle { get; set; }
        public List<ImportPage> SubPages { get; set; }
    }
}
