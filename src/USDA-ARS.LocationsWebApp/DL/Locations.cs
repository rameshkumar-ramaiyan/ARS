using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;

using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class Locations
    {

        private string locationModeCode;

        public string LocationModeCode
        {
            get { return locationModeCode; }
            set { locationModeCode = value; }
        }
        private string locationName;

        public string LocationName
        {
            get { return locationName; }
            set { locationName = value; }
        }
    }
}