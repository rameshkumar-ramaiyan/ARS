using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
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
using UmbracoExamine;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
	[PluginController("Usda")]
	public class NewsPickerController : UmbracoAuthorizedApiController
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;
		private static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);

		/// <summary>
		/// Returns a JSON string of search results for News Articles using custom Examine search index
		/// </summary>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public List<NewsSearchResult> Get(string query, string year = "")
		{
			List<NewsSearchResult> newsSearchResultList = null;

			try
			{
				if (false == string.IsNullOrWhiteSpace(query))
				{
					if (query == "undefined" || query == "null")
					{
						query = "";
					}
				}

				int yearInt = 0;

				if (int.TryParse(year, out yearInt))
				{ }


				newsSearchResultList = NewsSearchResults.GetNewsSearchResults(query, yearInt);
			}
			catch (Exception ex)
			{
				LogHelper.Error<NewsPickerController>("Usda News Picker Error", ex);
			}

			return newsSearchResultList;
		}


		/// <summary>
		/// Get a list of nodes from a comma seperated string of node ids
		/// </summary>
		/// <param name="nodeIds"></param>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public List<NewsItem> NodeList(string nodeIds)
		{
			List<NewsItem> newsList = new List<NewsItem>();

			if (false == string.IsNullOrWhiteSpace(nodeIds))
			{
				List<string> arrayList = nodeIds.Split(',').ToList();

				if (arrayList != null && arrayList.Any())
				{
					arrayList = arrayList.Distinct().ToList();
				}

				if (arrayList != null && arrayList.Any())
				{
					foreach (string nodeIdStr in arrayList)
					{
						int nodeId = 0;

						if (int.TryParse(nodeIdStr, out nodeId))
						{
							IPublishedContent node = UmbHelper.TypedContent(nodeId);

							if (node != null)
							{
								newsList.Add(new NewsItem() { Id = node.Id, Name = node.Name, Date = node.GetPropertyValue<DateTime>("articleDate") });
							}
						}
					}
				}
			}

			return newsList;
		}


		/// <summary>
		/// Get a list of years
		/// </summary>
		/// <param name="nodeIds"></param>
		/// <returns></returns>
		[System.Web.Http.AcceptVerbs("GET")]
		[System.Web.Http.HttpGet]
		public List<YearItem> YearsList()
		{
			List<YearItem> newsList = new List<YearItem>();

			IEnumerable<IPublishedContent> newsNodeList = Helpers.Nodes.NewsYearList();

			if (newsNodeList != null && newsNodeList.Any())
			{
				newsNodeList = newsNodeList.OrderByDescending(p => p.Name);

				foreach (IPublishedContent yearNode in newsNodeList)
				{
					newsList.Add(new YearItem(yearNode.Name, yearNode.Name));
				}
			}

			return newsList;
		}
	}

	public class NewsItem
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }
	}

	public class YearItem
	{
		public string Value { get; set; }
		public string Text { get; set; }

		public YearItem(string value, string text)
		{
			this.Value = value;
			this.Text = text;
		}
	}
}
