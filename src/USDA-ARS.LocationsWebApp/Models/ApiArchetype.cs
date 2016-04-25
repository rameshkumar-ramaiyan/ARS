using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace USDA_ARS.LocationsWebApp.Models
{
    public class ApiArchetype
    {
        public List<Fieldset> Fieldsets { get; set; }

    }

    public class Fieldset
    {
        public List<Property> Properties { get; set; }
        public string Alias { get; set; }
        public bool Disabled { get; set; }
        public Guid Id { get; set; }
    }

    public class Property
    {
        public string Alias { get; set; }
        public string Value { get; set; }

        public Property(string alias, string value)
        {
            this.Alias = alias;
            this.Value = value;
        }
    }

    public class Link
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }

        public Link(string name, string url, string icon)
        {
            this.Name = name;
            this.Url = url;
            this.Icon = icon;
        }
    }


    public class Site
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }

        public Site(string id, string name, string url, string icon)
        {
            this.Id = id;
            this.Name = name;
            this.Url = url;
            this.Icon = icon;
        }
    }

}