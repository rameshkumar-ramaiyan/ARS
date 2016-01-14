using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace USDA_ARS.LocationsWebApp.Models.Import
{
    public class Request
    {
        public string ApiKey { get; set; }
        public List<Content> ContentList { get; set; }
    }
}