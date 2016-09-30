﻿using Examine;
using Examine.SearchCriteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbracoExamine;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
	public class NewsSearchResults
	{
		public static List<NewsSearchResult> GetNewsSearchResults(string query, int year = 0)
		{
			List<NewsSearchResult> newsSearchResultList = new List<NewsSearchResult>();

			var Searcher = ExamineManager.Instance.SearchProviderCollection["NewsExternalSearcher"];
			ISearchCriteria searchCriteria = null;
			IEnumerable<SearchResult> searchResults = null;
			var fieldsToSearch = new[] { "nodeName", "bodyText", "newsBlurb" };

			if (false == string.IsNullOrWhiteSpace(query))
			{
				string[] queryArray = query.Split(' ');

				searchCriteria = Searcher.CreateSearchCriteria(Examine.SearchCriteria.BooleanOperation.Or);

				IBooleanOperation filter = searchCriteria.GroupedOr(fieldsToSearch, queryArray.First());

				foreach (var term in queryArray.Skip(1))
				{
					filter = filter.Or().GroupedOr(fieldsToSearch, term);
				}

				searchResults = Searcher.Search(filter.Compile()).OrderByDescending(x => x.Score);
			}
			else
			{
				searchCriteria = Searcher.CreateSearchCriteria();
				IBooleanOperation filter = searchCriteria.NodeTypeAlias("NewsArticle");
				searchResults = Searcher.Search(filter.Compile());
			}


			var noResults = searchResults.Count();

			if (searchResults != null && searchResults.Any())
			{
				foreach (SearchResult searchItem in searchResults)
				{
					DateTime articleDate = DateTime.MinValue;

					if (DateTime.TryParse(searchItem.Fields["articleDate"], out articleDate))
					{
						// All good!
					}

					NewsSearchResult newsSearchResultItem = new NewsSearchResult();

					newsSearchResultItem.Id = searchItem.Id;
					newsSearchResultItem.Name = searchItem.Fields["nodeName"];
					newsSearchResultItem.Date = articleDate;

					newsSearchResultList.Add(newsSearchResultItem);
				}

				if (year > 0)
				{
					DateTime startYear = DateTime.Parse("1/1/" + year);
					DateTime endYear = startYear.AddYears(1).AddDays(-1);

					newsSearchResultList = newsSearchResultList.Where(p => p.Date >= startYear && p.Date <= endYear).ToList();
            }

				if (true == string.IsNullOrWhiteSpace(query))
				{
					newsSearchResultList = newsSearchResultList.OrderByDescending(p => p.Date).Take(20).ToList();
            }
				else
				{
					newsSearchResultList = newsSearchResultList.Take(20).ToList();
            }
			}

			return newsSearchResultList;
		}
	}
}
