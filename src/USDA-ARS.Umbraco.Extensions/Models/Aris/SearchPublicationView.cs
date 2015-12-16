using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class SearchPublicationView
    {
        public List<SearchPublication> PublicationList { get; set; }
        public int Count { get; set; }
    }
}

