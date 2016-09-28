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
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
	[PluginController("Usda")]
	public class DupOldUrlsController : UmbracoApiController
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

		/// <summary>
		/// Gets the Help Information text for the Site Settings Umbraco node
		/// </summary>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public string Get()
		{
			string output = "";

			try
			{
				List<RedirectItem> nodesList = USDA_ARS.Umbraco.Extensions.Helpers.Nodes.NodesWithRedirectsList(false, false);

				nodesList = nodesList.GroupBy(x => x.OldUrl.Trim().ToLower()).Where(g => g.Skip(1).Any()).SelectMany(c => c).ToList();

				nodesList = nodesList.Where(p => false == string.IsNullOrWhiteSpace(p.OldUrl)).ToList();

				nodesList = nodesList.OrderBy(p => p.OldUrl).ToList();

				if (nodesList != null)
				{
					output += "<p><strong>Duplicate Old Urls Detected: " + nodesList.Count + "</strong></p>\r\n";

					if (nodesList.Any())
					{
						string tempOldUrl = nodesList[0].OldUrl.ToLower();

						output += "<ul>\r\n";

						foreach (RedirectItem item in nodesList)
						{
							IContent node = _contentService.GetById(item.UmbracoId);

							if (node != null)
							{
								string style = "";

								if (item.OldUrl.ToLower() != tempOldUrl)
								{
									style = "margin-top: 10px;";
								}

								output += "<li style=\""+ style +"\"><strong>"+ item.OldUrl + "</strong><br />Node: <a href=\"/umbraco/#/content/content/edit/"+ node.Id +"\">" + node.Name + "</a></li>\r\n";

								
							}

							tempOldUrl = item.OldUrl.ToLower();
                  }

						output += "<ul>\r\n";
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error<DataImporterController>("Usda Dup Detect Error", ex);
			}

			return output;
		}
	}
}
