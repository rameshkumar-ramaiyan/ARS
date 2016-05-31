using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
    public class ModeCodeFolderLookup
    {
        public string ModeCode { get; set; }
        public string FolderName { get; set; }
        public int UmbracoId { get; set; }
    }
}
