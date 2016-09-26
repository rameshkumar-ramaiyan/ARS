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
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
	[PluginController("Usda")]
	public class DocTypeListController : UmbracoApiController
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

		/// <summary>
		/// Get a list of doc types by Alias and Name
		/// </summary>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public string Get()
		{
			string output = "";

			try
			{
				List<DocTypeCms> docTypeList = null;
				List<DocTypeSelectItem> selectList = new List<DocTypeSelectItem>();

				var db = new Database("umbracoDbDSN");

				string sql = @"SELECT DISTINCT(Alias), Name
                                  FROM [cmsPropertyType]
                                  WHERE dataTypeId > 0
                                  ORDER BY Name";

				docTypeList = db.Query<DocTypeCms>(sql).ToList();

				if (docTypeList != null && docTypeList.Any())
				{
					foreach (DocTypeCms docType in docTypeList)
					{
						selectList.Add(new DocTypeSelectItem(docType.Alias, docType.Name));
					}
				}

				output = JsonConvert.SerializeObject(selectList);
			}
			catch (Exception ex)
			{
				LogHelper.Error<DataImporterController>("Usda Doc Type List Request Error", ex);
			}

			return output;
		}
	}

	public class DocTypeSelectItem
	{
		public string Alias { get; set; }
		public string Name { get; set; }

		public DocTypeSelectItem(string alias, string name)
		{
			this.Alias = alias;
			this.Name = name;
		}
	}


	public class DocTypeCms
	{
		[Column("Alias")]
		public string Alias { get; set; }

		[Column("Name")]
		[NullSetting(NullSetting = NullSettings.Null)]
		public string Name { get; set; }
	}
}
