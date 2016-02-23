using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public class TopicPickerValue
    {
        public string Value { get; set; }
        public string Text { get; set; }

        public TopicPickerValue(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }
    }
}
