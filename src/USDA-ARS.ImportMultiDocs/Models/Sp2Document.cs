using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportMultidocs.Models
{
	public class Sp2Document
	{
		public int DocId { get; set; }
		public string Title { get; set; }
		public int DocPage { get; set; }
		public string DocType { get; set; }
		public string OriginSiteType { get; set; }
		public string OriginSiteId { get; set; }
		public string Keywords { get; set; }
		public string HtmlHeader { get; set; }
		public bool DisplayTitle { get; set; }
		public string BodyText { get; set; }
	}
}
