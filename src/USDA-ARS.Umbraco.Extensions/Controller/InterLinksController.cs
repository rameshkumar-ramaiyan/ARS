using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
	[PluginController("Usda")]
	public class InterLinksController : UmbracoApiController
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;


		/// <summary>
		/// Gets a JSON string list of interlinks for a Umbraco ID (News Article)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public string Get(string id)
		{
			string output = "";

			try
			{
				IContent node = _contentService.GetById(Convert.ToInt32(id));

				if (node != null)
				{
					List<NewsInterLink> interLinkList = NewsInterLinks.GetLinksByNewsArticle(node.Id);
					List<InterLinksView> interLinksViewList = new List<InterLinksView>();

					if (interLinkList != null && interLinkList.Count > 0)
					{
						foreach (NewsInterLink linkItem in interLinkList)
						{
							InterLinksView viewItem = new InterLinksView();

							viewItem.Type = linkItem.LinkType;
							viewItem.Id = linkItem.LinkId;
							viewItem.Description = null;

							if (linkItem.LinkType == "person")
							{
								PeopleInfo person = People.GetPerson(Convert.ToInt32(linkItem.LinkId));

								if (person != null)
								{
									viewItem.Description = person.LastName + ", " + person.FirstName;
								}
								else
								{
									viewItem.Description = "Person not found";
								}
							}
							else if (linkItem.LinkType == "place")
							{
								IPublishedContent nodePlace = Nodes.GetNodeByModeCode(linkItem.LinkId.ToString(), false);

								if (nodePlace != null)
								{
									viewItem.Description = nodePlace.Name;
								}
								else
								{
									viewItem.Description = "Site not found";
								}
							}

							if (false == string.IsNullOrEmpty(viewItem.Description))
							{
								interLinksViewList.Add(viewItem);
							}

						}

						output = JsonConvert.SerializeObject(interLinksViewList);
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error<InterLinksController>("Usda InterLinks Error", ex);
			}

			return output;
		}
	}
}
