using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class Property
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public Property(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
