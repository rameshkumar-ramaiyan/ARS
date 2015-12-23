using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Content Content { get; set; }
    }
}
