using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public class TopicPickerItem
    {
        public TopicPickerValue Value { get; set; }
        public string Text { get; set; }

        public TopicPickerItem(string value, string text)
        {
            this.Value = new TopicPickerValue(value, text);
            this.Text = text;
        }
    }
}
