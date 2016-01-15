using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public List<ApiContent> ContentList { get; set; }
    }
}

