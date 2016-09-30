﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportMultiDocs.Models
{
   public class ImportPage
   {
      public string Title { get; set; }
      public string BodyText { get; set; }
      public string OldDocType { get; set; }
      public int OldDocId { get; set; }
      public int PageNumber { get; set; }
      public bool DisableTitle { get; set; }
      public string HtmlHeader { get; set; }
      public string Keywords { get; set; }
      public string ParentSiteCode { get; set; }
      public List<ImportPage> SubPages { get; set; }
   }
}