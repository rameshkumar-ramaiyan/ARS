﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Import
{
    public class ApiRequest
    {
        public string ApiKey { get; set; }
        public List<ApiContent> ContentList { get; set; }
    }
}