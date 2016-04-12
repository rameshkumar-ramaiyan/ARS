using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class PeopleListController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]

        public string Get(string id)
        {
            string output = "";

            try
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                IPublishedContent node = umbracoHelper.TypedContent(id);

                if (id != "0")
                {

                    if (node.DocumentTypeAlias == "PersonSite")
                    {
                        node = node.Parent.Parent;
                    }

                    if (node != null)
                    {
                        List<PeopleByCity> peopleList = new List<PeopleByCity>();

                        peopleList = Helpers.Aris.People.GetPeopleByCity(node.GetPropertyValue<string>("modeCode"));

                        if (peopleList != null)
                        {
                            peopleList = peopleList.OrderBy(p => p.LastName).ThenBy(t => t.FirstName).ToList();

                            List<PeopleSelectItem> selectList = new List<PeopleSelectItem>();

                            selectList.Add(new PeopleSelectItem("", ""));

                            foreach (var person in peopleList)
                            {
                                selectList.Add(new PeopleSelectItem(person.PersonId.ToString(), person.LastName + ", " + person.FirstName + " <" + person.Email + ">  ("+ Helpers.ModeCodes.ModeCodeAddDashes(person.ModeCodeConcat) +")"));
                            }

                            output = JsonConvert.SerializeObject(selectList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda Download Request Error", ex);
            }

            return output;
        }
    }

    public class PeopleSelectItem
    {
        public string Id { get; set; }
        public string Person { get; set; }

        public PeopleSelectItem(string id, string person)
        {
            this.Id = id;
            this.Person = person;
        }
    }
}
