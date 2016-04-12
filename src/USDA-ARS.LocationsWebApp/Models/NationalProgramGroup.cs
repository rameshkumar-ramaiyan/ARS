using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace USDA_ARS.LocationsWebApp.Models
{
    public class NationalProgramGroup
    {
        public int NPGroupId { get; set; }
        public int UmbracoId { get; set; }
        public string ModeCode { get; set; }
        public string Name { get; set; }
    }
}