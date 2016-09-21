using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
	public class Redirects
	{
		private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

		public static string RedirectUrl(string badUrl)
		{
			string redirectUrl = null;

			if (false == string.IsNullOrEmpty(badUrl))
			{
				if (false == badUrl.ToLower().StartsWith("/umbraco/") && false == badUrl.ToLower().StartsWith("/__browserlink/"))
				{
					// Magazine
					redirectUrl = RedirectMagazineUrl(badUrl);

					// Redirect Pages
					if (true == string.IsNullOrEmpty(redirectUrl))
					{
						redirectUrl = RedirectOldPages(badUrl);
					}

					// Redirects In Umbraco
					if (true == string.IsNullOrEmpty(redirectUrl))
					{
						redirectUrl = RedirectPageStoredInUmbraco(badUrl);
					}

					// Redirects with Doc ID
					if (true == string.IsNullOrEmpty(redirectUrl))
					{
						redirectUrl = RedirectWithDocId(badUrl);
					}

					// Redirects with SP2UserFiles
					if (true == string.IsNullOrEmpty(redirectUrl))
					{
						redirectUrl = RedirectSP2UserFilesUrl(badUrl);
					}
				}
			}


			return redirectUrl;
		}


		/// <summary>
		/// Redirect ARS Magazine URLs
		/// </summary>
		/// <param name="badUrl"></param>
		/// <returns></returns>
		public static string RedirectMagazineUrl(string badUrl)
		{
			string redirectUrl = null;

			// Is this a AgMagazine URL?
			if (badUrl.ToLower().IndexOf("/is/ar") >= 0)
			{
				// /is/AR/2016/mar16/cookies.htm

				List<string> badUrlArray = badUrl.ToLower().Split('/').ToList();

				if (badUrlArray != null && badUrlArray.Count == 6)
				{
					string agMagUrl = "https://agresearchmag.ars.usda.gov/";

					int index = 4;

					string month = badUrlArray[index].Substring(0, 3);
					string year = badUrlArray[index].Substring(3, 2);

					int yearInt = 0;

					if (int.TryParse(year, out yearInt))
					{
						if (yearInt <= 99 && yearInt >= 50)
						{
							year = "19" + year;
						}
						else
						{
							year = "20" + year;
						}

						agMagUrl += year + "/";
						agMagUrl += month + "/";
						index++;

						string endUrl = badUrlArray[index].Replace(".htm", "");

						if (false == string.IsNullOrEmpty(endUrl) && false == endUrl.ToLower().EndsWith(".pdf"))
						{
							if (endUrl.Length >= 4)
							{
								endUrl = endUrl.Substring(0, endUrl.Length - 4);
							}
						}

						agMagUrl += endUrl;

						if (endUrl.ToLower().EndsWith(".pdf"))
						{
							agMagUrl = agMagUrl.Replace("https://agresearchmag.ars.usda.gov/", "https://agresearchmag.ars.usda.gov/ar/archive/");
						}

						redirectUrl = agMagUrl;
					}
					else
					{
						LogHelper.Warn(typeof(Redirects), "Invalid format for Mag Redirect. URL: " + badUrl);
					}
				}
			}

			return redirectUrl;
		}


		/// <summary>
		/// Redirects for random/dynamic pages
		/// </summary>
		/// <param name="badUrl"></param>
		/// <returns></returns>
		public static string RedirectOldPages(string badUrl)
		{
			string redirectUrl = null;

			if (badUrl.ToLower().IndexOf("/main/site_main.htm?modecode=") >= 0)
			{
				string modeCodeFind = badUrl.ToLower().Replace("/main/site_main.htm?modecode=", "");
				IPublishedContent findNode = Nodes.GetNodeByModeCode(modeCodeFind);

				if (findNode != null)
				{
					redirectUrl = findNode.Url;
				}
			}

			if (badUrl.ToLower().IndexOf("/research/programs/usmap.htm?stateabbr=") >= 0 && badUrl.ToLower().IndexOf("&np_code=") >= 0)
			{
				// "/research/research-programs-by-state/?state=AR&npCode=107";
				IPublishedContent findNode = Nodes.GetNodeById(9130); // Research Programs by State node

				if (findNode != null)
				{
					Match m2 = Regex.Match(badUrl, @"stateabbr=([^&]*)&np_code=([\d]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
					if (m2.Success)
					{
						redirectUrl = findNode.Url + "?state=" + m2.Groups[1].Value + "&npCode=" + m2.Groups[2].Value;
					}
				}
			}


			if (badUrl.ToLower().IndexOf("/research/programs/program?npcode=") >= 0 || badUrl.ToLower().IndexOf("/research/programs/programs.htm?np_code=") >= 0)
			{
				string npCode = "";

				// /research/programs/programs.htm?NP_CODE=215
				Match m2 = Regex.Match(badUrl, @"np[_]*code=([\d]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

				if (m2.Success)
				{
					npCode = m2.Groups[1].Value;
				}

				if (false == string.IsNullOrWhiteSpace(npCode))
				{
					IPublishedContent nodeFound = Nodes.GetNodeByNpCode(npCode);

					if (nodeFound != null)
					{
						redirectUrl = nodeFound.Url;
					}
				}
			}


			// If it didn't find, look through list of custom redirects
			if (true == string.IsNullOrEmpty(redirectUrl))
			{
				List<RedirectToNode> redirectList = GetListOfRedirects();

				if (redirectList != null && redirectList.Any())
				{
					foreach (RedirectToNode redirectNode in redirectList)
					{
						if (true == string.IsNullOrEmpty(redirectUrl))
						{
							if (badUrl.ToLower().IndexOf(redirectNode.OldUrl.ToLower()) >= 0)
							{
								IPublishedContent nodeFound = Nodes.GetNodeById(redirectNode.UmbracoId);

								if (nodeFound != null)
								{
									redirectUrl = badUrl.ToLower().Replace(redirectNode.OldUrl, nodeFound.Url + redirectNode.AppendString);
								}
							}
						}
					}
				}

			}

			return redirectUrl;
		}


		public static List<RedirectToNode> GetListOfRedirects()
		{
			List<RedirectToNode> redirectList = new List<RedirectToNode>();

			// MAKE SURE EVERYTHING IS IN LOWER CASE!
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/locations/npslocation.htm?modecode=", UmbracoId = 9127, AppendString = "?modeCode=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/locations/locations.htm?modecode=", UmbracoId = 200481, AppendString = "?modeCode=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/people.htm?personid=", UmbracoId = 6992, AppendString = "?person-id=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/research/projects/projects.htm?accn_no=", UmbracoId = 8092, AppendString = "?accnNo=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/services/ttbrowse.htm?stp_code=", UmbracoId = 8101, AppendString = "?stpCode=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/services/software/download.htm?softwareid=", UmbracoId = 8066, AppendString = "?softwareid=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/research/publications/publications.htm?seq_no_115=", UmbracoId = 9114, AppendString = "?seqNo115=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/research/projects_programs.htm?modecode=", UmbracoId = 8089, AppendString = "?modeCode=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/research/programs/programs.htm?projectlist=true&NP_CODE=", UmbracoId = 000, AppendString = "?" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/address.htm?personid=", UmbracoId = 200341, AppendString = "?person-id=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/news.htm?personid=", UmbracoId = 200348, AppendString = "?person-id=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/projects.htm?personid=", UmbracoId = 200349, AppendString = "?person-id=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/publications.htm?personid=", UmbracoId = 200350, AppendString = "?person-id=" });

			redirectList.Add(new RedirectToNode() { OldUrl = "/research/programs/programs.htm?projectlist=true&np_code=", UmbracoId = 200759, AppendString = "?npCode=" });
			redirectList.Add(new RedirectToNode() { OldUrl = "/research/programs/programs.htm?list421s=true&np_code=", UmbracoId = 23458, AppendString = "?npCode=" });

			redirectList.Add(new RedirectToNode() { OldUrl = "/research/projectsbylocation.htm?modecode=", UmbracoId = 8095, AppendString = "?modeCode=" });



			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
			//redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });

			// /research/programs/usmap.htm?stateabbr=OK&NP_CODE=101 
			// /research/research-programs-by-state/?state=AR&npCode=107

			return redirectList;
		}


		/// <summary>
		/// Redirects with Umbraco pages that have a Old URL
		/// </summary>
		/// <param name="badUrl"></param>
		/// <returns></returns>
		public static string RedirectPageStoredInUmbraco(string badUrl)
		{
			string redirectUrl = null;

			List<RedirectItem> nodesList = Nodes.NodesWithRedirectsList();

			if (nodesList != null && nodesList.Any())
			{
				RedirectItem redirectItem = nodesList.Where(p => p.OldUrl == badUrl.ToLower()).FirstOrDefault();

				if (redirectItem != null)
				{
					int umbracoId = redirectItem.UmbracoId;

					IPublishedContent foundRedirectNode = Nodes.GetNodeById(umbracoId);

					if (foundRedirectNode != null)
					{
						redirectUrl = foundRedirectNode.Url;
					}
				}
			}

			return redirectUrl;
		}


		/// <summary>
		/// Redirects that have a Doc ID in Umbraco
		/// </summary>
		/// <param name="badUrl"></param>
		/// <returns></returns>
		public static string RedirectWithDocId(string badUrl)
		{
			string redirectUrl = null;

			if (badUrl.ToLower().IndexOf("docid=") >= 0)
			{
				int docId = 0;

				Match m2 = Regex.Match(badUrl, @"docid=([\d]*)", RegexOptions.Singleline);
				if (m2.Success)
				{
					if (int.TryParse(m2.Groups[1].Value, out docId))
					{
						IPublishedContent docNode = Nodes.GetNodeByOldDocId(docId.ToString());

						if (docNode != null)
						{
							redirectUrl = docNode.Url;
						}
					}
				}
			}

			return redirectUrl;
		}


		public static string RedirectSP2UserFilesUrl(string badUrl)
		{
			string redirectUrl = null;

			if (badUrl.ToLower().Contains("/sp2userfiles/"))
			{
				redirectUrl = badUrl;

				redirectUrl = Regex.Replace(redirectUrl, @"/sp2userfiles/place", "/ARSUserFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/sp2userfiles/people", "/ARSUserFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/sp2userfiles/person", "/ARSUserFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/sp2userfiles/Program/", "/ARSUserFiles/np", RegexOptions.IgnoreCase);

				redirectUrl = Regex.Replace(redirectUrl, @"/ARSUserFiles/news/", "/ARSUserFiles/oc/", RegexOptions.IgnoreCase);

				redirectUrl = Regex.Replace(redirectUrl, @"""/News/Docs\.htm\?docid\=23712""", "\"/{localLink:8002}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/News/Docs\.htm\?docid\=23559""", "\"/{localLink:1145}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/is/graphics/photos/""", "\"/{localLink:1145}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/is/pr/index\.html""", "\"/{localLink:6996}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/News/docs\.htm\?docid\=6697""", "\"/{localLink:9134}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/News/docs\.htm\?docid\=1383""", "\"/{localLink:8030}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/News/docs\.htm\?docid\=1281""", "\"/{localLink:8003}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/news/events\.htm""", "\"/{localLink:8024}\"", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"""/news/events\.htm""", "\"/{localLink:8024}\"", RegexOptions.IgnoreCase);

				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/sciQualRev", "/ARSUserFiles/OSQR", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/HQsubsite", "/ARSUserFiles/NACOP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/Careers", "/ARSUserFiles/Careers", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/irp", "/ARSUserFiles/OIRP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/odeo", "/ARSUserFiles/ODEO", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Place/01090000", "/ARSUserFiles/OTT", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Subsite/CEAP", "/ARSUserFiles/CEAP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Place", "/ARSUserFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Language", "/ARSUserFiles/Language", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Program", "/ARSUserFiles/Program", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/Person", "/ARSUserFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/02020000StewardsCEAPsites", "/ARSUserFiles/CEAP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/02060000NationalProgramCycle", "/ARSUserFiles/02060000/NationalProgramCycle", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000Awards", "/ARSUserFiles/80000000/Awards", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000BAAwards", "/ARSUserFiles/80000000/BAAwards", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000BADiversityTaskforce", "/ARSUserFiles/80000000/BADiversityTaskforce", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000BARCCentennial", "/ARSUserFiles/80000000/BARCCentennial", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000BARCPublicFieldDay", "/ARSUserFiles/80000000/BARCPublicFieldDay", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000beef", "/ARSUserFiles/80000000/beef", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000Biocontrol4Kids", "/ARSUserFiles/80000000/Biocontrol4Kids", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000ChoptankWatershed", "/ARSUserFiles/80000000/ChoptankWatershed", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000CinnamonHealthBenefits", "/ARSUserFiles/80000000/CinnamonHealthBenefits", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000CivilRights", "/ARSUserFiles/80000000/CivilRights", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000CurrentNews", "/ARSUserFiles/80000000/CurrentNews", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000DistinguishedLecture", "/ARSUserFiles/80000000/DistinguishedLecture", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000DistinguishedLectureSeries", "/ARSUserFiles/80000000/DistinguishedLectureSeries", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000FARB", "/ARSUserFiles/80000000/FARB", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000FMMI", "/ARSUserFiles/80000000/FMMI", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000FriendsofArgiculturalResearch", "/ARSUserFiles/80000000/FriendsofArgiculturalResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000GenomicsGeneMapping", "/ARSUserFiles/80000000/GenomicsGeneMapping", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000HotResearchTopics", "/ARSUserFiles/80000000/HotResearchTopics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000Jillsresearch", "/ARSUserFiles/80000000/Jillsresearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000OutreachDiversityandEO", "/ARSUserFiles/80000000/OutreachDiversityandEO", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000SpatialWorkshop", "/ARSUserFiles/80000000/SpatialWorkshop", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000SystematicsResearch", "/ARSUserFiles/80000000/SystematicsResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000UniversityPartnerships", "/ARSUserFiles/80000000/UniversityPartnerships", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000USDAHistoryExhibit", "/ARSUserFiles/80000000/USDAHistoryExhibit", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000VisitorInformation", "/ARSUserFiles/80000000/VisitorInformation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12000000WorkplaceDiversity", "/ARSUserFiles/80000000/WorkplaceDiversity", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12301500TestSite1", "/ARSUserFiles/80200510/TestSite1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12301500testtest", "/ARSUserFiles/80200510/testtest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12350000Samplesubsite", "/ARSUserFiles/80400500/Samplesubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12351500CaveResearchProject", "/ARSUserFiles/80400505/CaveResearchProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12351500test", "/ARSUserFiles/80400505/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12352000HumanStudiesFacility", "/ARSUserFiles/80400510/HumanStudiesFacility", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000asdf", "/ARSUserFiles/80400530/asdf", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000asdfpig", "/ARSUserFiles/80400530/asdfpig", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000biquitiy", "/ARSUserFiles/80400530/biquitiy", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000dontworrythiswillgoawy", "/ARSUserFiles/80400530/dontworrythiswillgoawy", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000egg", "/ARSUserFiles/80400530/egg", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000fishtheprk", "/ARSUserFiles/80400530/fishtheprk", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000FMB", "/ARSUserFiles/80400530/FMB", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000horticulturepr", "/ARSUserFiles/80400530/horticulturepr", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000jabberwalki4ya", "/ARSUserFiles/80400530/jabberwalki4ya", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000liveandletlearn", "/ARSUserFiles/80400530/liveandletlearn", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000pigsnspce", "/ARSUserFiles/80400530/pigsnspce", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000pigsnspceFLtulnc3", "/ARSUserFiles/12355000pigsnspce/pigsnspceFLtulnc3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000test", "/ARSUserFiles/80400530/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12355000walktillyoudrop", "/ARSUserFiles/80400530/walktillyoudrop", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12454700USNationalFungusCollections", "/ARSUserFiles/80420575/USNationalFungusCollections", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650000VetServices", "/ARSUserFiles/12650000/VetServices", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650400Glomalin", "/ARSUserFiles/80420505/Glomalin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500BetterPigs", "/ARSUserFiles/12451700/BetterPigs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500GeneTherapy", "/ARSUserFiles/12451700/GeneTherapy", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500GeneTherapy2", "/ARSUserFiles/12451700/GeneTherapy2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500genetherapy2blah", "/ARSUserFiles/12451700/genetherapy2blah", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500Goats", "/ARSUserFiles/12451700/Goats", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500ProgenyTest", "/ARSUserFiles/12451700/ProgenyTest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500speclcharacters", "/ARSUserFiles/12451700/speclcharacters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500Test", "/ARSUserFiles/12451700/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500test2", "/ARSUserFiles/12451700/test2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500test2withspcialCh89rcters", "/ARSUserFiles/12451700/test2withspcialCh89rcters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500testprogeny", "/ARSUserFiles/12451700/testprogeny", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500testspecialcharacters", "/ARSUserFiles/12451700/testspecialcharacters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650500useofspacesspecialchracters", "/ARSUserFiles/12451700/useofspacesspecialchracters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600AndrewKim", "/ARSUserFiles/80420510/AndrewKim", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600ARSWaterDatabase", "/ARSUserFiles/80420510/ARSWaterDatabase", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600BayWatersheds", "/ARSUserFiles/80420510/BayWatersheds", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600Cesium137", "/ARSUserFiles/80420510/Cesium137", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600ChrisPooley", "/ARSUserFiles/80420510/ChrisPooley", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600CropConditionandYieldResearch", "/ARSUserFiles/80420510/CropConditionandYieldResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600JoannaBirch", "/ARSUserFiles/80420510/JoannaBirch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600JornadaExperiment", "/ARSUserFiles/80420510/JornadaExperiment", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600jornex", "/ARSUserFiles/80420510/jornex", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600keithsaxton", "/ARSUserFiles/80420510/keithsaxton", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600LarryCorp", "/ARSUserFiles/80420510/LarryCorp", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600Monsoon90", "/ARSUserFiles/80420510/Monsoon90", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600NicksRainfallData", "/ARSUserFiles/80420510/NicksRainfallData", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600OPE3", "/ARSUserFiles/80420510/OPE3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600ResearchScientists", "/ARSUserFiles/80420510/ResearchScientists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600RSatBARC", "/ARSUserFiles/80420510/RSatBARC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600RSBasics", "/ARSUserFiles/80420510/RSBasics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600SoilMoistureProgram", "/ARSUserFiles/80420510/SoilMoistureProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600TugrulYilmaz", "/ARSUserFiles/80420510/TugrulYilmaz", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12650600WaterDataCenter", "/ARSUserFiles/80420510/WaterDataCenter", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12654000AvianCoccidiosis", "/ARSUserFiles/80420515/AvianCoccidiosis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12654000BovineNeosporosis", "/ARSUserFiles/80420515/BovineNeosporosis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12654000Toxoplasmosis", "/ARSUserFiles/80420515/Toxoplasmosis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655200filtrexx", "/ARSUserFiles/12655200/filtrexx", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655300DownloadCodeEnvironmentalTransport", "/ARSUserFiles/80420525/DownloadCodeEnvironmentalTransport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655300EnvironmentalTransport", "/ARSUserFiles/80420525/EnvironmentalTransport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655300EnvironmentalTransportCode", "/ARSUserFiles/80420525/EnvironmentalTransportCode", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655300EnvironmentalTransportCodeDownload", "/ARSUserFiles/80420525/EnvironmentalTransportCodeDownload", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12655300VisitingandPostdoctoralResearchers", "/ARSUserFiles/80420525/VisitingandPostdoctoralResearchers", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12657500ISLResearchAssociates", "/ARSUserFiles/12657500/ISLResearchAssociates", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12657500PublicationLists", "/ARSUserFiles/12657500/PublicationLists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900111", "/ARSUserFiles/12659900/111", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900222", "/ARSUserFiles/12659900/222", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest1", "/ARSUserFiles/12659900/Alphatest1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900AlphaTest10", "/ARSUserFiles/12659900/AlphaTest10", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900AlphaTest11", "/ARSUserFiles/12659900/AlphaTest11", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest2", "/ARSUserFiles/12659900/Alphatest2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest3", "/ARSUserFiles/12659900/Alphatest3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest4", "/ARSUserFiles/12659900/Alphatest4", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900AlphaTest5", "/ARSUserFiles/12659900/AlphaTest5", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest6", "/ARSUserFiles/12659900/Alphatest6", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest7", "/ARSUserFiles/12659900/Alphatest7", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900AlphaTest8", "/ARSUserFiles/12659900/AlphaTest8", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Alphatest9", "/ARSUserFiles/12659900/Alphatest9", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Anderson", "/ARSUserFiles/12659900/Anderson", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900AnimalMetabolism", "/ARSUserFiles/12659900/AnimalMetabolism", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900bahamas", "/ARSUserFiles/12659900/bahamas", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900banres", "/ARSUserFiles/12659900/banres", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Barnes", "/ARSUserFiles/12659900/Barnes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BestTest3", "/ARSUserFiles/12659900/BestTest3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest10", "/ARSUserFiles/12659900/betatest10", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest11", "/ARSUserFiles/12659900/betatest11", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest12", "/ARSUserFiles/12659900/betatest12", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest12BetaTest12", "/ARSUserFiles/12659900BetaTest12/BetaTest12BetaTest12", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest12BetaTest12test", "/ARSUserFiles/12659900BetaTest12BetaTest12/BetaTest12BetaTest12test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest13", "/ARSUserFiles/12659900/Betatest13", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest14", "/ARSUserFiles/12659900/betatest14", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest15", "/ARSUserFiles/12659900/Betatest15", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest16", "/ARSUserFiles/12659900/Betatest16", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest17", "/ARSUserFiles/12659900/BetaTest17", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest18", "/ARSUserFiles/12659900/BetaTest18", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest19", "/ARSUserFiles/12659900/BetaTest19", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest2", "/ARSUserFiles/12659900/Betatest2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest20", "/ARSUserFiles/12659900/BetaTest20", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest21", "/ARSUserFiles/12659900/BetaTest21", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest22", "/ARSUserFiles/12659900/BetaTest22", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest23", "/ARSUserFiles/12659900/BetaTest23", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetaTest24", "/ARSUserFiles/12659900/BetaTest24", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest25", "/ARSUserFiles/12659900/betatest25", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betaTest26", "/ARSUserFiles/12659900/betaTest26", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest27", "/ARSUserFiles/12659900/betatest27", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest4", "/ARSUserFiles/12659900/Betatest4", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest5", "/ARSUserFiles/12659900/betatest5", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest6", "/ARSUserFiles/12659900/betatest6", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Betatest7", "/ARSUserFiles/12659900/Betatest7", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest8", "/ARSUserFiles/12659900/betatest8", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900betatest9", "/ARSUserFiles/12659900/betatest9", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900BetsTest1", "/ARSUserFiles/12659900/BetsTest1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900blah", "/ARSUserFiles/12659900/blah", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Blueberries", "/ARSUserFiles/12659900/Blueberries", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900boyd", "/ARSUserFiles/12659900/boyd", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Bradford", "/ARSUserFiles/12659900/Bradford", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Brann", "/ARSUserFiles/12659900/Brann", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900cattleprod", "/ARSUserFiles/12659900/cattleprod", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900cattleprodding", "/ARSUserFiles/12659900/cattleprodding", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900CHAN", "/ARSUserFiles/12659900/CHAN", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900cole", "/ARSUserFiles/12659900/cole", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900coleaboutus", "/ARSUserFiles/12659900/coleaboutus", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900COLEY", "/ARSUserFiles/12659900/COLEY", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Comer", "/ARSUserFiles/12659900/Comer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900coombes", "/ARSUserFiles/12659900/coombes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900daniels", "/ARSUserFiles/12659900/daniels", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Dean", "/ARSUserFiles/12659900/Dean", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Degen", "/ARSUserFiles/12659900/Degen", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Dimmer", "/ARSUserFiles/12659900/Dimmer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900dreher", "/ARSUserFiles/12659900/dreher", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900dreher22", "/ARSUserFiles/12659900/dreher22", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Duplantis", "/ARSUserFiles/12659900/Duplantis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900eggs", "/ARSUserFiles/12659900/eggs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900EntomologyResearch", "/ARSUserFiles/12659900/EntomologyResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Erickson1", "/ARSUserFiles/12659900/Erickson1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Ferrin", "/ARSUserFiles/12659900/Ferrin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900finaltest123", "/ARSUserFiles/12659900/finaltest123", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900finaltest123testagain", "/ARSUserFiles/12659900finaltest123/finaltest123testagain", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900finaltest2", "/ARSUserFiles/12659900/finaltest2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Finaltesting1", "/ARSUserFiles/12659900/Finaltesting1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900foobar", "/ARSUserFiles/12659900/foobar", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900foodsource", "/ARSUserFiles/12659900/foodsource", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Fox", "/ARSUserFiles/12659900/Fox", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900FoxChild", "/ARSUserFiles/12659900/FoxChild", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900FuturePartneringPlans", "/ARSUserFiles/12659900/FuturePartneringPlans", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900GAPAResearch", "/ARSUserFiles/12659900/GAPAResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Glynn", "/ARSUserFiles/12659900/Glynn", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900GlynnChild", "/ARSUserFiles/12659900/GlynnChild", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900GoatBreeding", "/ARSUserFiles/12659900/GoatBreeding", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900goattest1", "/ARSUserFiles/12659900/goattest1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Grauke", "/ARSUserFiles/12659900/Grauke", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900graves", "/ARSUserFiles/12659900/graves", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Griggs", "/ARSUserFiles/12659900/Griggs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900groves", "/ARSUserFiles/12659900/groves", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900guidry", "/ARSUserFiles/12659900/guidry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900h2", "/ARSUserFiles/12659900/h2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Hagin", "/ARSUserFiles/12659900/Hagin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900HARDING", "/ARSUserFiles/12659900/HARDING", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Harvey", "/ARSUserFiles/12659900/Harvey", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900henderson", "/ARSUserFiles/12659900/henderson", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900heydon", "/ARSUserFiles/12659900/heydon", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Houser", "/ARSUserFiles/12659900/Houser", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Housergoat", "/ARSUserFiles/12659900/Housergoat", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900HouserHouser", "/ARSUserFiles/12659900Houser/HouserHouser", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900jbarrientes", "/ARSUserFiles/12659900/jbarrientes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Kammlah", "/ARSUserFiles/12659900/Kammlah", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900kirby", "/ARSUserFiles/12659900/kirby", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Koesterman", "/ARSUserFiles/12659900/Koesterman", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900konstas", "/ARSUserFiles/12659900/konstas", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Kuntz", "/ARSUserFiles/12659900/Kuntz", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Lasttest", "/ARSUserFiles/12659900/Lasttest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Lasttest1", "/ARSUserFiles/12659900/Lasttest1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900lkjhlkjhlkjh", "/ARSUserFiles/12659900/lkjhlkjhlkjh", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900LocationInformation", "/ARSUserFiles/12659900/LocationInformation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Lynn", "/ARSUserFiles/12659900/Lynn", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900malik", "/ARSUserFiles/12659900/malik", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Meyer", "/ARSUserFiles/12659900/Meyer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MilkGoats", "/ARSUserFiles/12659900/MilkGoats", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Miska", "/ARSUserFiles/12659900/Miska", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mitchell2", "/ARSUserFiles/12659900/Mitchell2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Morian", "/ARSUserFiles/12659900/Morian", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MuhlK", "/ARSUserFiles/12659900/MuhlK", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Murray", "/ARSUserFiles/12659900/Murray", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite", "/ARSUserFiles/12659900/Mysubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite1", "/ARSUserFiles/12659900/Mysubsite1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite123", "/ARSUserFiles/12659900/Mysubsite123", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite1234", "/ARSUserFiles/12659900/Mysubsite1234", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite12345", "/ARSUserFiles/12659900/Mysubsite12345", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysubsite123456", "/ARSUserFiles/12659900/Mysubsite123456", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MySubsiteMYsubsite", "/ARSUserFiles/12659900MySubsite/MySubsiteMYsubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MySubsiteMySusbite", "/ARSUserFiles/12659900MySubsiteMYsubsite/MySubsiteMySusbite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MySubsiteMySusbiteMySubsite", "/ARSUserFiles/12659900MySubsiteMySusbite/MySubsiteMySusbiteMySubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900MySubsiteMySusbiteMySubsiteMySubsite", "/ARSUserFiles/12659900MySubsiteMySusbiteMySubsite/MySubsiteMySusbiteMySubsiteMySubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Mysusbite14", "/ARSUserFiles/12659900/Mysusbite14", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Nearpass", "/ARSUserFiles/12659900/Nearpass", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900NewTestsite", "/ARSUserFiles/12659900/NewTestsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900paul", "/ARSUserFiles/12659900/paul", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Pentecost", "/ARSUserFiles/12659900/Pentecost", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900philpot", "/ARSUserFiles/12659900/philpot", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Plasko", "/ARSUserFiles/12659900/Plasko", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900portales", "/ARSUserFiles/12659900/portales", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Proctor", "/ARSUserFiles/12659900/Proctor", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900raspberries", "/ARSUserFiles/12659900/raspberries", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Renner", "/ARSUserFiles/12659900/Renner", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Resendes", "/ARSUserFiles/12659900/Resendes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Roberts", "/ARSUserFiles/12659900/Roberts", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Robles", "/ARSUserFiles/12659900/Robles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Roses", "/ARSUserFiles/12659900/Roses", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900RosesRoses", "/ARSUserFiles/12659900Roses/RosesRoses", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900SCHLEIDER", "/ARSUserFiles/12659900/SCHLEIDER", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900schleiderchild", "/ARSUserFiles/12659900/schleiderchild", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Schweikert", "/ARSUserFiles/12659900/Schweikert", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900SclerotiniaInitiative", "/ARSUserFiles/12659900/SclerotiniaInitiative", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900SeftonK", "/ARSUserFiles/12659900/SeftonK", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900sgarcia", "/ARSUserFiles/12659900/sgarcia", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Siebenaler", "/ARSUserFiles/12659900/Siebenaler", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Stern", "/ARSUserFiles/12659900/Stern", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Stiefel", "/ARSUserFiles/12659900/Stiefel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900strawberries", "/ARSUserFiles/12659900/strawberries", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900subscharfenstein", "/ARSUserFiles/12659900/subscharfenstein", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test", "/ARSUserFiles/12659900/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test000", "/ARSUserFiles/12659900/Test000", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test060210", "/ARSUserFiles/12659900/test060210", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test1", "/ARSUserFiles/12659900/Test1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test101", "/ARSUserFiles/12659900/test101", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test102", "/ARSUserFiles/12659900/test102", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test102test103", "/ARSUserFiles/12659900test102/test102test103", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test103", "/ARSUserFiles/12659900/Test103", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test104", "/ARSUserFiles/12659900/test104", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test111", "/ARSUserFiles/12659900/Test111", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test123", "/ARSUserFiles/12659900/test123", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test123456", "/ARSUserFiles/12659900/Test123456", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test13", "/ARSUserFiles/12659900/Test13", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test1317", "/ARSUserFiles/12659900/test1317", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test1342", "/ARSUserFiles/12659900/test1342", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test143", "/ARSUserFiles/12659900/test143", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test1621", "/ARSUserFiles/12659900/test1621", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test1622", "/ARSUserFiles/12659900/test1622", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test2", "/ARSUserFiles/12659900/Test2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test207", "/ARSUserFiles/12659900/Test207", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test222", "/ARSUserFiles/12659900/Test222", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test2test2", "/ARSUserFiles/12659900test2/test2test2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test3", "/ARSUserFiles/12659900/test3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test333", "/ARSUserFiles/12659900/Test333", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test4", "/ARSUserFiles/12659900/test4", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test420", "/ARSUserFiles/12659900/Test420", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test444", "/ARSUserFiles/12659900/test444", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test5", "/ARSUserFiles/12659900/test5", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test567", "/ARSUserFiles/12659900/Test567", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test567Test34", "/ARSUserFiles/12659900Test567/Test567Test34", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test6", "/ARSUserFiles/12659900/Test6", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test666", "/ARSUserFiles/12659900/Test666", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test712010", "/ARSUserFiles/12659900/test712010", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test714", "/ARSUserFiles/12659900/test714", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test7141", "/ARSUserFiles/12659900/test7141", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900test72010", "/ARSUserFiles/12659900/test72010", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Test999", "/ARSUserFiles/12659900/Test999", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testAAA", "/ARSUserFiles/12659900/testAAA", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testabc", "/ARSUserFiles/12659900/Testabc", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testafter5", "/ARSUserFiles/12659900/Testafter5", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testafter51", "/ARSUserFiles/12659900/Testafter51", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testafter51Testafter52", "/ARSUserFiles/12659900Testafter5-1/Testafter51Testafter52", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testafter5old", "/ARSUserFiles/12659900/testafter5old", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testafter5oldmethod", "/ARSUserFiles/12659900/testafter5oldmethod", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900tester1", "/ARSUserFiles/12659900/tester1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testersparadise", "/ARSUserFiles/12659900/Testersparadise", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testingtimesagain", "/ARSUserFiles/12659900/Testingtimesagain", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testpage", "/ARSUserFiles/12659900/Testpage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestPage420", "/ARSUserFiles/12659900/TestPage420", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testrun1", "/ARSUserFiles/12659900/testrun1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testrun2", "/ARSUserFiles/12659900/testrun2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testsite", "/ARSUserFiles/12659900/Testsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestSite1", "/ARSUserFiles/12659900/TestSite1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testsite2", "/ARSUserFiles/12659900/Testsite2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testSite3", "/ARSUserFiles/12659900/testSite3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsiteTest8", "/ARSUserFiles/12659900Testsite/TestsiteTest8", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsiteTest8Test8", "/ARSUserFiles/12659900TestsiteTest8/TestsiteTest8Test8", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsiteTest8Test8test8", "/ARSUserFiles/12659900TestsiteTest8Test8/TestsiteTest8Test8test8", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsiteTest8Test8test8Test11", "/ARSUserFiles/12659900TestsiteTest8Test8test8/TestsiteTest8Test8test8Test11", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsiteTest8Test8test8Test11Test12", "/ARSUserFiles/12659900TestsiteTest8Test8test8Test11/TestsiteTest8Test8test8Test11Test12", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testsiteusingcfc", "/ARSUserFiles/12659900/testsiteusingcfc", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Testsubsite", "/ARSUserFiles/12659900/Testsubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900TestsubsiteTestsite12345", "/ARSUserFiles/12659900Testsubsite/TestsubsiteTestsite12345", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900testwithcfc", "/ARSUserFiles/12659900/testwithcfc", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Tucker", "/ARSUserFiles/12659900/Tucker", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900VadsSubsite", "/ARSUserFiles/12659900/VadsSubsite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Vadstest", "/ARSUserFiles/12659900/Vadstest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Walker", "/ARSUserFiles/12659900/Walker", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Wanner", "/ARSUserFiles/12659900/Wanner", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900wientjes", "/ARSUserFiles/12659900/wientjes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Wilcox", "/ARSUserFiles/12659900/Wilcox", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900wolfe", "/ARSUserFiles/12659900/wolfe", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12659900Zornes", "/ARSUserFiles/12659900/Zornes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100AccesstoPlantQuarantineTesting", "/ARSUserFiles/12751100/AccesstoPlantQuarantineTesting", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Autumnberry", "/ARSUserFiles/12751100/Autumnberry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Blueberry", "/ARSUserFiles/12751100/Blueberry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Citrus", "/ARSUserFiles/12751100/Citrus", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100CitrusYellowMosaicVirus", "/ARSUserFiles/12751100/CitrusYellowMosaicVirus", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Crasnberry", "/ARSUserFiles/12751100/Crasnberry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100fhblueberryregen", "/ARSUserFiles/12751100/fhblueberryregen", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100fhgenedelivery", "/ARSUserFiles/12751100/fhgenedelivery", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100fhinvitro", "/ARSUserFiles/12751100/fhinvitro", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100newblues", "/ARSUserFiles/12751100/newblues", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100PGQOPlantGermplasmQuarantineOffice", "/ARSUserFiles/12751100/PGQOPlantGermplasmQuarantineOffice", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100PlantGermplasmQuarantineOffice", "/ARSUserFiles/12751100/PlantGermplasmQuarantineOffice", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Prohibitedgenera", "/ARSUserFiles/12751100/Prohibitedgenera", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100QuarantineCapacity", "/ARSUserFiles/12751100/QuarantineCapacity", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100QuarantineInformation", "/ARSUserFiles/12751100/QuarantineInformation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751100Strawberry", "/ARSUserFiles/12751100/Strawberry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751500PIBooks", "/ARSUserFiles/80420545/PIBooks", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751800DiscoverSynthesizeandDevelopAttractants", "/ARSUserFiles/80420550/DiscoverSynthesizeandDevelopAttractants", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751800EnhancedBiologicalPotency", "/ARSUserFiles/80420550/EnhancedBiologicalPotency", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12751800EnhancementandSuppressionofInsects", "/ARSUserFiles/80420550/EnhancementandSuppressionofInsects", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752000BlueberryReleases", "/ARSUserFiles/80420555/BlueberryReleases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752000BlueberryResearch", "/ARSUserFiles/80420555/BlueberryResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752000index", "/ARSUserFiles/80420555/index", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752500Spiroplasmakunkeliigenomesequencingproject", "/ARSUserFiles/80420560/Spiroplasmakunkeliigenomesequencingproject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752500Spiroplasmakunkelligenomesequencingproject", "/ARSUserFiles/80420560/Spiroplasmakunkelligenomesequencingproject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12752900NematologyLaboratoryHistory", "/ARSUserFiles/80420565/NematologyLaboratoryHistory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12753100ChristopherPooley", "/ARSUserFiles/80420570/ChristopherPooley", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12753900Gnomoniaceae", "/ARSUserFiles/12454700/Gnomoniaceae", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12753900MikhailSogonov", "/ARSUserFiles/12454700/MikhailSogonov", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12753900USNationalFungusCollections", "/ARSUserFiles/12454700/USNationalFungusCollections", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12753900USNationalSeedHerbarium", "/ARSUserFiles/12454700/USNationalSeedHerbarium", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100Acari", "/ARSUserFiles/80420580/Acari", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100AphidCardFile", "/ARSUserFiles/80420580/AphidCardFile", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100CollectingandPreservingInsectsandMites", "/ARSUserFiles/80420580/CollectingandPreservingInsectsandMites", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100CollectingandPreservingInsectsMites", "/ARSUserFiles/80420580/CollectingandPreservingInsectsMites", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100IDService", "/ARSUserFiles/80420580/IDService", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100LeafhopperChecklist", "/ARSUserFiles/80420580/LeafhopperChecklist", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100PyraloideaLarvaeKey", "/ARSUserFiles/80420580/PyraloideaLarvaeKey", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12754100Test1", "/ARSUserFiles/80420580/Test1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12755100BSSGWorkshop", "/ARSUserFiles/80420520/BSSGWorkshop", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12755100CottonDatabase", "/ARSUserFiles/80420520/CottonDatabase", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12755100DatabaseFiles", "/ARSUserFiles/80420520/DatabaseFiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/12755100FullTextPublicationspdf", "/ARSUserFiles/80420520/FullTextPublicationspdf", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19000000EEOCivilRights", "/ARSUserFiles/19000000/EEOCivilRights", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19000000NAACOP", "/ARSUserFiles/19000000/NAACOP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19000000SafetyHealthandEnvironmentalTraining", "/ARSUserFiles/19000000/SafetyHealthandEnvironmentalTraining", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19000000SHE", "/ARSUserFiles/19000000/SHE", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19020000index", "/ARSUserFiles/80700500/index", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19020000indexResearch", "/ARSUserFiles/19020000index/indexResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19020000MainLevel1", "/ARSUserFiles/80700500/MainLevel1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19020000Software", "/ARSUserFiles/80700500/Software", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19020000SoftwareModel", "/ARSUserFiles/80700500/SoftwareModel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070000LocationSupportUnit", "/ARSUserFiles/80620500/LocationSupportUnit", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500BradburyPersonalBio", "/ARSUserFiles/80620500/BradburyPersonalBio", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500CRISProjects", "/ARSUserFiles/80620500/CRISProjects", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500HonorsandAwards", "/ARSUserFiles/80620500/HonorsandAwards", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500Pseudomonassyringaesystemsbiology", "/ARSUserFiles/80620500/Pseudomonassyringaesystemsbiology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500RuminalFermentation", "/ARSUserFiles/80620500/RuminalFermentation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070500YongYang", "/ARSUserFiles/80620500/YongYang", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19070505FunctionalandComparativeProteomicsCenter", "/ARSUserFiles/80620505/FunctionalandComparativeProteomicsCenter", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19100000ContactInformation", "/ARSUserFiles/80600000/ContactInformation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19150500NEPotatoTechnologyForum", "/ARSUserFiles/80300500/NEPotatoTechnologyForum", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19151000OurLocation", "/ARSUserFiles/80301000/OurLocation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19200000ForeignDiseaseWeedSicenceResearch", "/ARSUserFiles/80440500/ForeignDiseaseWeedSicenceResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19200000WeedBiologicalControlProgram", "/ARSUserFiles/80440500/WeedBiologicalControlProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19260000AsianLonghornedBeetleResearch", "/ARSUserFiles/80100500/AsianLonghornedBeetleResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19260000BiologicalControlofEmeraldAshBorer", "/ARSUserFiles/80100500/BiologicalControlofEmeraldAshBorer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19300000Flavobacterium2007", "/ARSUserFiles/80820500/Flavobacterium2007", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19320000Farms", "/ARSUserFiles/19320000/Farms", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19320000HistoricalPerspectives", "/ARSUserFiles/19320000/HistoricalPerspectives", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19320000Jill", "/ARSUserFiles/19320000/Jill", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19320000PosterGallery", "/ARSUserFiles/19320000/PosterGallery", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19350000DairyFunctions", "/ARSUserFiles/80720500/DairyFunctions", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19350000EngineeringSUPERSupportGroup", "/ARSUserFiles/80720500/EngineeringSUPERSupportGroup", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19350000NationalHistoricChemicalLandmark", "/ARSUserFiles/80720500/NationalHistoricChemicalLandmark", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19350000Poultry", "/ARSUserFiles/80720500/Poultry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19350000TechnologyTransfer", "/ARSUserFiles/80720500/TechnologyTransfer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19353000PathogenModelingProgramPMP", "/ARSUserFiles/19353000/PathogenModelingProgramPMP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19353700CenterforExcellence", "/ARSUserFiles/80720510/CenterforExcellence", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19353700PathogenModelingProgram", "/ARSUserFiles/80720510/PathogenModelingProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19354700CEEPR", "/ARSUserFiles/80720515/CEEPR", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/19355700BiomassPyrolysisResearch", "/ARSUserFiles/80720520/BiomassPyrolysisResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20321500StressBiology", "/ARSUserFiles/20321500/StressBiology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20340515StoredProductInsects", "/ARSUserFiles/20340515/StoredProductInsects", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20340515StoredProductInsectsStoredProductInsects", "/ARSUserFiles/20340515StoredProductInsects/StoredProductInsectsStoredProductInsects", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20600500AridLandsEcologyLab", "/ARSUserFiles/20600500/AridLandsEcologyLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20600500GerminationProfiles", "/ARSUserFiles/20600500/GerminationProfiles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20600500JeffGicklhorn", "/ARSUserFiles/20600500/JeffGicklhorn", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/20902000ReportsandProgress", "/ARSUserFiles/20902000/ReportsandProgress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30121500ThomasTrout", "/ARSUserFiles/30121500/ThomasTrout", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30123020JasonYoung", "/ARSUserFiles/30123020/JasonYoung", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30180500AdaptiveGrazingManagement", "/ARSUserFiles/30180500/AdaptiveGrazingManagement", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30180500ThunderBasinGrassland", "/ARSUserFiles/30180500/ThunderBasinGrassland", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30400500CattleandSheepGenomicData", "/ARSUserFiles/30400500/CattleandSheepGenomicData", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30400500Genomics", "/ARSUserFiles/30400500/Genomics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30620500EpigeneticsSymposium2015", "/ARSUserFiles/30620500/EpigeneticsSymposium2015", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/30620500Solicitations", "/ARSUserFiles/30620500/Solicitations", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36000000TechnologyTransfer", "/ARSUserFiles/50000000/TechnologyTransfer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36021500CPIDS", "/ARSUserFiles/50201000/CPIDS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36021500USLEDatabase", "/ARSUserFiles/50201000/USLEDatabase", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36021500WEPP", "/ARSUserFiles/50201000/WEPP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36040500AboutUs", "/ARSUserFiles/50800500/AboutUs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000AgriculturalEngineering", "/ARSUserFiles/50820500/AgriculturalEngineering", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000GreenhouseProductionResearch", "/ARSUserFiles/50820500/GreenhouseProductionResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000GreenhouseProductionResearchGroup", "/ARSUserFiles/50820500/GreenhouseProductionResearchGroup", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000HighlightedResearch", "/ARSUserFiles/50820500/HighlightedResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000HorticulturalInsects", "/ARSUserFiles/50820500/HorticulturalInsects", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000IntelligentSpraySystems", "/ARSUserFiles/50820500/IntelligentSpraySystems", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000IR4FoodUseandOrnamentals", "/ARSUserFiles/50820500/IR4FoodUseandOrnamentals", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000PlantPathology", "/ARSUserFiles/50820500/PlantPathology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36071000USDAARSResearchWeatherNetwork", "/ARSUserFiles/50820500/USDAARSResearchWeatherNetwork", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36113500LaboratoryResearch", "/ARSUserFiles/50121000/LaboratoryResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36220000DiverseMaizeResearch", "/ARSUserFiles/50700000/DiverseMaizeResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500CaveResearchProject", "/ARSUserFiles/50701000/CaveResearchProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500CavesProject", "/ARSUserFiles/50701000/CavesProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500CavesProjectCavesProject", "/ARSUserFiles/36221500CavesProject/CavesProjectCavesProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500GreenComputing", "/ARSUserFiles/50701000/GreenComputing", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500GreenComputing_X", "/ARSUserFiles/50701000/GreenComputing_X", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36221500test", "/ARSUserFiles/50701000/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36222000DivereseMaizeResearch", "/ARSUserFiles/50701500/DivereseMaizeResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36222000DiverseMaizeResearch", "/ARSUserFiles/50701500/DiverseMaizeResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000AAACRACBooks", "/ARSUserFiles/36250000/AAACRACBooks", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000AACRAC", "/ARSUserFiles/36250000/AACRAC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000AACRACTest", "/ARSUserFiles/36250000/AACRACTest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000AACRACVideos", "/ARSUserFiles/36250000/AACRACVideos", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000AACRC", "/ARSUserFiles/36250000/AACRC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000NewAACRAC", "/ARSUserFiles/36250000/NewAACRAC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000Proteomics", "/ARSUserFiles/36250000/Proteomics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000SafetyISUUnits", "/ARSUserFiles/36250000/SafetyISUUnits", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000test", "/ARSUserFiles/36250000/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36250000Testsite1", "/ARSUserFiles/36250000/Testsite1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251000MaizeGDB", "/ARSUserFiles/50300500/MaizeGDB", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251000SoyBase", "/ARSUserFiles/50300500/SoyBase", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200AmaranthProgram", "/ARSUserFiles/50301000/AmaranthProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200BrennerPublications", "/ARSUserFiles/50301000/BrennerPublications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200EntomologyProgram", "/ARSUserFiles/50301000/EntomologyProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200FacilitiesProgram", "/ARSUserFiles/50301000/FacilitiesProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200GEMFieldOperations", "/ARSUserFiles/50301000/GEMFieldOperations", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200GEMInfoTechProgram", "/ARSUserFiles/50301000/GEMInfoTechProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200GEMProgram", "/ARSUserFiles/50301000/GEMProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200HorticultureProgram", "/ARSUserFiles/50301000/HorticultureProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200NCRPISStaff", "/ARSUserFiles/50301000/NCRPISStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200NPGSAshConservationProject", "/ARSUserFiles/50301000/NPGSAshConservationProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200PlantPathologyProgram", "/ARSUserFiles/50301000/PlantPathologyProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200SeedStorageProgram", "/ARSUserFiles/50301000/SeedStorageProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251200SeedViabilityProgram", "/ARSUserFiles/50301000/SeedViabilityProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36251510CoverCrops", "/ARSUserFiles/36251510/CoverCrops", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000bkjlkjhkljhkljh", "/ARSUserFiles/50302000/bkjlkjhkljhkljh", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000InternationalSymposium", "/ARSUserFiles/50302000/InternationalSymposium", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000kllkjljk", "/ARSUserFiles/50302000/kllkjljk", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000MeetingManyHostsofMycobacterium", "/ARSUserFiles/50302000/MeetingManyHostsofMycobacterium", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000testps", "/ARSUserFiles/50302000/testps", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36253000VirtualConference", "/ARSUserFiles/50302000/VirtualConference", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400000HealthandSafety", "/ARSUserFiles/50620000/HealthandSafety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400000Maillists", "/ARSUserFiles/50620000/Maillists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Barberry", "/ARSUserFiles/50620500/Barberry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500BarberryResearch", "/ARSUserFiles/50620500/BarberryResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Bibliographies", "/ARSUserFiles/50620500/Bibliographies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500CDLDatabases", "/ARSUserFiles/50620500/CDLDatabases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500CDLDatabasesCDLDatabases", "/ARSUserFiles/36400500CDLDatabases/CDLDatabasesCDLDatabases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Cerealrustbulletins", "/ARSUserFiles/50620500/Cerealrustbulletins", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Cerealrusts", "/ARSUserFiles/50620500/Cerealrusts", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Cooperatorspage", "/ARSUserFiles/50620500/Cooperatorspage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Databases", "/ARSUserFiles/3640050/Databases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Fusarium", "/ARSUserFiles/50620500/Fusarium", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Germplasmevaluation", "/ARSUserFiles/50620500/Germplasmevaluation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500HealthandSafety", "/ARSUserFiles/50620500/HealthandSafety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Maillists", "/ARSUserFiles/50620500/Maillists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Monitoringofrustspores", "/ARSUserFiles/50620500/Monitoringofrustspores", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Othersites", "/ARSUserFiles/50620500/Othersites", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Publications", "/ARSUserFiles/50620500/Publications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Resistancegenes", "/ARSUserFiles/50620500/Resistancegenes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500Smallgrainlossesduetorust", "/ARSUserFiles/50620500/Smallgrainlossesduetorust", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36400500testchild", "/ARSUserFiles/50620500/testchild", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36402000Research", "/ARSUserFiles/50621500/Research", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/36402000RodneyVenterea", "/ARSUserFiles/50621500/RodneyVenterea", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/50820500InteligentSpray", "/ARSUserFiles/50820500/InteligentSpray", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53000000PWAOfficeSupportAdvisoryCouncil", "/ARSUserFiles/20000000/PWAOfficeSupportAdvisoryCouncil", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500DirectorsMessage", "/ARSUserFiles/20340500/DirectorsMessage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500MeetDirector", "/ARSUserFiles/20340500/MeetDirector", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500MeettheDirector", "/ARSUserFiles/20340500/MeettheDirector", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500Message", "/ARSUserFiles/20340500/Message", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500MessageMessagefromtheDirector", "/ARSUserFiles/53021500Message/MessageMessagefromtheDirector", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021500SJVASCResearchUnits", "/ARSUserFiles/20340500/SJVASCResearchUnits", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021505PandPdocshtmdocid16398", "/ARSUserFiles/20340505/PandPdocshtmdocid16398", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021505VacantNematologist", "/ARSUserFiles/20340505/VacantNematologist", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021515test", "/ARSUserFiles/20340510/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565Fumigation", "/ARSUserFiles/20340515/Fumigation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565Insectary", "/ARSUserFiles/20340515/Insectary", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565Insectary2", "/ARSUserFiles/20340515/Insectary2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565NavelOrangewormAreawide", "/ARSUserFiles/20340515/NavelOrangewormAreawide", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565NavelOrangewormAreawideProject", "/ARSUserFiles/20340515/NavelOrangewormAreawideProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53021565SpottedWingDrosophilaDevelopment", "/ARSUserFiles/20340515/SpottedWingDrosophilaDevelopment", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53061500AlternativesToMethylBromide", "/ARSUserFiles/20320500/AlternativesToMethylBromide", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53061500EtiologyandBiologyofTCD", "/ARSUserFiles/20320500/EtiologyandBiologyofTCD", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53061500RiceGenetics", "/ARSUserFiles/20320500/RiceGenetics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53061500SustainableFloriculture", "/ARSUserFiles/20320500/SustainableFloriculture", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53061500SustainableViticulture", "/ARSUserFiles/20320500/SustainableViticulture", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000Actinidia", "/ARSUserFiles/20321000/Actinidia", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000ActinidiaInventory", "/ARSUserFiles/20321000/ActinidiaInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000CudraniaInventory", "/ARSUserFiles/20321000/CudraniaInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000DistributionPoliciesProcedures", "/ARSUserFiles/20321000/DistributionPoliciesProcedures", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000FicusPage", "/ARSUserFiles/20321000/FicusPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000FigInventory", "/ARSUserFiles/20321000/FigInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000GrapeHybridInventory", "/ARSUserFiles/20321000/GrapeHybridInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000GrapePage", "/ARSUserFiles/20321000/GrapePage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000GrapeRelativesInventory", "/ARSUserFiles/20321000/GrapeRelativesInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000GrapeSSRFingerprinting", "/ARSUserFiles/20321000/GrapeSSRFingerprinting", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000JuglanInventory", "/ARSUserFiles/20321000/JuglanInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000JuglanPage", "/ARSUserFiles/20321000/JuglanPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000KiwiPage", "/ARSUserFiles/20321000/KiwiPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000MorusInventory", "/ARSUserFiles/20321000/MorusInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000MorusPage", "/ARSUserFiles/20321000/MorusPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000NationalPlantGermplasmSystem", "/ARSUserFiles/20321000/NationalPlantGermplasmSystem", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000OleaInventory", "/ARSUserFiles/20321000/OleaInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000OleaPage", "/ARSUserFiles/20321000/OleaPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000OrderForm", "/ARSUserFiles/20321000/OrderForm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PersimmonInventory", "/ARSUserFiles/20321000/PersimmonInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PersimmonPage", "/ARSUserFiles/20321000/PersimmonPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PictureoftheWeek", "/ARSUserFiles/20321000/PictureoftheWeek", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PistachioPage", "/ARSUserFiles/20321000/PistachioPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PistaciaInventory", "/ARSUserFiles/20321000/PistaciaInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PomegranateInventory", "/ARSUserFiles/20321000/PomegranateInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusAlmondInventory", "/ARSUserFiles/20321000/PrunusAlmondInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusApricotInventory", "/ARSUserFiles/20321000/PrunusApricotInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusCherryInventory", "/ARSUserFiles/20321000/PrunusCherryInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusPage", "/ARSUserFiles/20321000/PrunusPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusPeachInventory", "/ARSUserFiles/20321000/PrunusPeachInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusPlumcotInventory", "/ARSUserFiles/20321000/PrunusPlumcotInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PrunusPlumInventory", "/ARSUserFiles/20321000/PrunusPlumInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PterocaryaInventory", "/ARSUserFiles/20321000/PterocaryaInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PunicaPage", "/ARSUserFiles/20321000/PunicaPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000PureGrapeSpeciesInventory", "/ARSUserFiles/20321000/PureGrapeSpeciesInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000RootstockInventory", "/ARSUserFiles/20321000/RootstockInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000TableGrapeInventory", "/ARSUserFiles/20321000/TableGrapeInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000VitisPage", "/ARSUserFiles/20321000/VitisPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062000WineGrapeInventory", "/ARSUserFiles/20321000/WineGrapeInventory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062500Directions", "/ARSUserFiles/20321500/Directions", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062500LocalCareers", "/ARSUserFiles/20321500/LocalCareers", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53062500METABOLOMICS", "/ARSUserFiles/20321500/METABOLOMICS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53200345indexAboutUs", "/ARSUserFiles/53200345/indexAboutUs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53250000BioBasedProducts", "/ARSUserFiles/20300500/BioBasedProducts", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53250000CommunityGarden", "/ARSUserFiles/20300500/CommunityGarden", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200PotatoMolecularGenetics", "/ARSUserFiles/20300515/PotatoMolecularGenetics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200Test", "/ARSUserFiles/20300515/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200Test2", "/ARSUserFiles/20300515/Test2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200TestSite", "/ARSUserFiles/20300515/TestSite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200UnderConstruction", "/ARSUserFiles/20300515/UnderConstruction", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200WheatQualityLaboratory", "/ARSUserFiles/20300515/WheatQualityLaboratory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53253200WheatQualityResearch", "/ARSUserFiles/20300515/WheatQualityResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254100FFG", "/ARSUserFiles/20300530/FFG", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254100FungalFunctionalGenomics", "/ARSUserFiles/20300530/FungalFunctionalGenomics", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254100SortingDetection", "/ARSUserFiles/20300530/SortingDetection", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300Centaureasolstitialis", "/ARSUserFiles/20300535/Centaureasolstitialis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300DavisCA", "/ARSUserFiles/20300535/DavisCA", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300Ludwigiahexapetala", "/ARSUserFiles/20300535/Ludwigiahexapetala", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300RenoNV", "/ARSUserFiles/20300535/RenoNV", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300Tamarixspp", "/ARSUserFiles/20300535/Tamarixspp", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300test", "/ARSUserFiles/20300535/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53254300Thistle", "/ARSUserFiles/20300535/Thistle", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Admin", "/ARSUserFiles/53410000/Admin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000AdminandSupport", "/ARSUserFiles/53410000/AdminandSupport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000AdministrationLocationSupportStaff", "/ARSUserFiles/53410000/AdministrationLocationSupportStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000AdministrationSupport", "/ARSUserFiles/53410000/AdministrationSupport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000AdminSupport", "/ARSUserFiles/53410000/AdminSupport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Aquaculture", "/ARSUserFiles/53410000/Aquaculture", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000ArcticPlantGermplasmResearch", "/ARSUserFiles/53410000/ArcticPlantGermplasmResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000FishByProductUtilization", "/ARSUserFiles/53410000/FishByProductUtilization", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Germplasm", "/ARSUserFiles/53410000/Germplasm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000IntegratedPestManagement", "/ARSUserFiles/53410000/IntegratedPestManagement", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Support", "/ARSUserFiles/53410000/Support", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Test", "/ARSUserFiles/53410000/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000Testadmin", "/ARSUserFiles/53410000/Testadmin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53410000UtilizationofFishByproducts", "/ARSUserFiles/53410000/UtilizationofFishByproducts", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53420300WesternApiculturalSociety", "/ARSUserFiles/20220500/WesternApiculturalSociety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53440500PinkBollworm", "/ARSUserFiles/53440500/PinkBollworm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53440500test", "/ARSUserFiles/53440500/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53470000ALARCPublicationDatabase", "/ARSUserFiles/20200500/ALARCPublicationDatabase", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53470000PestBibliographies", "/ARSUserFiles/20200500/PestBibliographies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53470000Software", "/ARSUserFiles/20200500/Software", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53471005Bemisiatabaci", "/ARSUserFiles/20200505/Bemisiatabaci", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53471005PBW", "/ARSUserFiles/20200505/PBW", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53471015Staff", "/ARSUserFiles/20200515/Staff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53480000ARSPullmanEnvironmentalManagementSystem", "/ARSUserFiles/20900000/ARSPullmanEnvironmentalManagementSystem", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53481500GAMarxPeaGeneticStockCollection", "/ARSUserFiles/20901500/GAMarxPeaGeneticStockCollection", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53481500MedicagotrunculataGermplasmCollection", "/ARSUserFiles/20901500/MedicagotrunculataGermplasmCollection", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53481500PhaseolusGeneticStockCollection", "/ARSUserFiles/20901500/PhaseolusGeneticStockCollection", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53481500SafflowerGeneticResources", "/ARSUserFiles/20901500/SafflowerGeneticResources", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/534asdf", "/ARSUserFiles/534/534asdf", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53500000ITSpecialist", "/ARSUserFiles/20940500/ITSpecialist", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53500000Scientists", "/ARSUserFiles/20940500/Scientists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000Alfalfa", "/ARSUserFiles/20960500/Alfalfa", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000Beans", "/ARSUserFiles/20960500/Beans", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000BioFuel", "/ARSUserFiles/20960500/BioFuel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000Peas", "/ARSUserFiles/20960500/Peas", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000Potatoes", "/ARSUserFiles/20960500/Potatoes", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53540000Weeds", "/ARSUserFiles/20960500/Weeds", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53560000cpcrc", "/ARSUserFiles/20740500/cpcrc", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53560500Canolamatic", "/ARSUserFiles/20740500/Canolamatic", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53580000Safety", "/ARSUserFiles/20720000/Safety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53580000SafetyHealthandEnvironmentalManagement", "/ARSUserFiles/20720000/SafetyHealthandEnvironmentalManagement", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53580500HopsResearch", "/ARSUserFiles/20720500/HopsResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53580500NewportORWorksite", "/ARSUserFiles/20720500/NewportORWorksite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53580500ResearchScientists", "/ARSUserFiles/20720500/ResearchScientists", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581000FoliarPathology", "/ARSUserFiles/20721000/FoliarPathology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581000FoodChemistry", "/ARSUserFiles/20721000/FoodChemistry", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581000GrapeResearch", "/ARSUserFiles/20721000/GrapeResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581000PseudomonasFluorescensPf5", "/ARSUserFiles/20721000/PseudomonasFluorescensPf5", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581000SmallFruitBreeding", "/ARSUserFiles/20721000/SmallFruitBreeding", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581500AnnualReports", "/ARSUserFiles/20721500/AnnualReports", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581500Events", "/ARSUserFiles/20721500/Events", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581500OpenHouse", "/ARSUserFiles/20721500/OpenHouse", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53581500PublicationsinPDF", "/ARSUserFiles/20721500/PublicationsinPDF", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53620000ClarkAnimalTrackingSystem", "/ARSUserFiles/20520500/ClarkAnimalTrackingSystem", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53620000ClarkGPSCollars", "/ARSUserFiles/20520500/ClarkGPSCollars", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53620000DataReports", "/ARSUserFiles/20520500/DataReports", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53620000Models", "/ARSUserFiles/20520500/Models", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53620000RCEWDataReports", "/ARSUserFiles/20520500/RCEWDataReports", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53660000CORE", "/ARSUserFiles/20500500/CORE", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53660000CORE1", "/ARSUserFiles/20500500/CORE1", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53680000NWISRLPublicationList", "/ARSUserFiles/20540500/NWISRLPublicationList", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53680000PAM", "/ARSUserFiles/20540500/PAM", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53680000Presentations", "/ARSUserFiles/20540500/Presentations", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500AnimalEcologyLab", "/ARSUserFiles/20600500/AnimalEcologyLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500BryceWehan", "/ARSUserFiles/20600500/BryceWehan", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Clementsposters", "/ARSUserFiles/20600500/Clementsposters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500DanHarmon", "/ARSUserFiles/20600500/DanHarmon", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500ecophysiology", "/ARSUserFiles/20600500/ecophysiology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500EntomologyLab", "/ARSUserFiles/20600500/EntomologyLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500errosionlabphotos", "/ARSUserFiles/20600500/errosionlabphotos", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Errosionlabpubs", "/ARSUserFiles/20600500/Errosionlabpubs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500FayAllen", "/ARSUserFiles/20600500/FayAllen", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500GeneticsLab", "/ARSUserFiles/20600500/GeneticsLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Hydrologyerrosionlab", "/ARSUserFiles/20600500/Hydrologyerrosionlab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500hydrologylabtechs", "/ARSUserFiles/20600500/hydrologylabtechs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500KirkTonkel", "/ARSUserFiles/20600500/KirkTonkel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Kossinuowakpo", "/ARSUserFiles/20600500/Kossinuowakpo", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500LindsayDimitri", "/ARSUserFiles/20600500/LindsayDimitri", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500LonglandPosters", "/ARSUserFiles/20600500/LonglandPosters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Newspage", "/ARSUserFiles/20600500/Newspage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500Officestaff", "/ARSUserFiles/20600500/Officestaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500RangelandRehabiliatationLab", "/ARSUserFiles/20600500/RangelandRehabiliatationLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500RangelandRehabilitationLab", "/ARSUserFiles/20600500/RangelandRehabilitationLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500rehabfieldphotos", "/ARSUserFiles/20600500/rehabfieldphotos", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500RehabposterSRM", "/ARSUserFiles/20600500/RehabposterSRM", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500RehabposterWSWS", "/ARSUserFiles/20600500/RehabposterWSWS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500RehabWeedScienceposters", "/ARSUserFiles/20600500/RehabWeedScienceposters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500SandraLi", "/ARSUserFiles/20600500/SandraLi", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500SeedLab", "/ARSUserFiles/20600500/SeedLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500SoilScience", "/ARSUserFiles/20600500/SoilScience", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500SoilScienceLab", "/ARSUserFiles/20600500/SoilScienceLab", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500soilslabtechs", "/ARSUserFiles/20600500/soilslabtechs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500soilwaterconservationposters", "/ARSUserFiles/20600500/soilwaterconservationposters", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500TechnicalSupport", "/ARSUserFiles/20600500/TechnicalSupport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500TimJones", "/ARSUserFiles/20600500/TimJones", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500TyeMorgan", "/ARSUserFiles/20600500/TyeMorgan", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500welcomepage", "/ARSUserFiles/20600500/welcomepage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/53700500welcomepagewelcomepage", "/ARSUserFiles/53700500welcomepage/welcomepagewelcomepage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54000000COWYResourceTeam", "/ARSUserFiles/30000000/COWYResourceTeam", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54000000NaturalResourcesResearchUpdateSystem", "/ARSUserFiles/30000000/NaturalResourcesResearchUpdateSystem", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54000000NPALeadershipConference2011", "/ARSUserFiles/30000000/NPALeadershipConference2011", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54000000NPASAC", "/ARSUserFiles/30000000/NPASAC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54000000WhiteMoldResearch", "/ARSUserFiles/30000000/WhiteMoldResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500Links", "/ARSUserFiles/30120500/Links", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500NationalAnimalGermplasmProgram", "/ARSUserFiles/30120500/NationalAnimalGermplasmProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGeneticResourcesPreservationProgam", "/ARSUserFiles/30120500/PlantGeneticResourcesPreservationProgam", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGeneticResourcesPreservationProgram", "/ARSUserFiles/30120500/PlantGeneticResourcesPreservationProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGermplasm", "/ARSUserFiles/30120500/PlantGermplasm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGermplasmPreservation", "/ARSUserFiles/30120500/PlantGermplasmPreservation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGermplasmPreservationResearch", "/ARSUserFiles/30120500/PlantGermplasmPreservationResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020500PlantGermplasmPreservationResearchUnit", "/ARSUserFiles/30120500/PlantGermplasmPreservationResearchUnit", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020700Disney", "/ARSUserFiles/30121000/Disney", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020700NitrogenTools", "/ARSUserFiles/30121000/NitrogenTools", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020700NREL", "/ARSUserFiles/30121000/NREL", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54020700SPNRPublications", "/ARSUserFiles/30121000/SPNRPublications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021000FieldDay2014", "/ARSUserFiles/30121500/FieldDay2014", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021000Personalpages", "/ARSUserFiles/30121500/Personalpages", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021000PersonalpagesPersonalpages", "/ARSUserFiles/54021000Personalpages/PersonalpagesPersonalpages", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021000Personalsites", "/ARSUserFiles/30121500/Personalsites", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021500bcv", "/ARSUserFiles/30122000/bcv", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021500people", "/ARSUserFiles/30122000/people", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54021500PHZM", "/ARSUserFiles/30122000/PHZM", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54022000GermplasmReleases", "/ARSUserFiles/30122500/GermplasmReleases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54090000FACEProject", "/ARSUserFiles/54092500/FACEProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54090000PHACE", "/ARSUserFiles/30180500/PHACE", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54090000RemoteSensing", "/ARSUserFiles/30180500/RemoteSensing", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54092500USDAClimateHub", "/ARSUserFiles/30180500/USDAClimateHub", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54280500Cucurbita", "/ARSUserFiles/20800500/Cucurbita", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54280500DrGriswoldsPublications", "/ARSUserFiles/20800500/DrGriswoldsPublications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54281000MBFT2012", "/ARSUserFiles/20801000/MBFT2012", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54282000Bulletin415USWesternStatesPoisonousPlants", "/ARSUserFiles/20801500/Bulletin415USWesternStatesPoisonousPlants", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54282000Bulletin415WesternUSPoisonousPlants", "/ARSUserFiles/20801500/Bulletin415WesternUSPoisonousPlants", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54282000LaboratorySafety", "/ARSUserFiles/20801500/LaboratorySafety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54300520PackFactorStudy", "/ARSUserFiles/30200525/PackFactorStudy", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54300530testKF", "/ARSUserFiles/30200530/testKF", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54340000ARSMontana", "/ARSUserFiles/30300500/ARSMontana", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000AdministrationLocationSupportStaff", "/ARSUserFiles/30320500/AdministrationLocationSupportStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Archive", "/ARSUserFiles/30320500/Archive", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Awards", "/ARSUserFiles/30320500/Awards", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000CommunityInfo", "/ARSUserFiles/30320500/CommunityInfo", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000ConferenceArchive", "/ARSUserFiles/30320500/ConferenceArchive", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000EMS", "/ARSUserFiles/30320500/EMS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000EraseyourEwaste", "/ARSUserFiles/30320500/EraseyourEwaste", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Ewaste", "/ARSUserFiles/30320500/Ewaste", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000History", "/ARSUserFiles/30320500/History", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000IrrigationInfo", "/ARSUserFiles/30320500/IrrigationInfo", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000JustForKids", "/ARSUserFiles/30320500/JustForKids", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000LighterSide", "/ARSUserFiles/30320500/LighterSide", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Links", "/ARSUserFiles/30320500/Links", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000MonDakInfo", "/ARSUserFiles/30320500/MonDakInfo", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000MovieGallery", "/ARSUserFiles/30320500/MovieGallery", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Movies", "/ARSUserFiles/30320500/Movies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000NationalResearch", "/ARSUserFiles/30320500/NationalResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000NewsArchive", "/ARSUserFiles/30320500/NewsArchive", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000OrganizationalChart", "/ARSUserFiles/30320500/OrganizationalChart", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Outreach", "/ARSUserFiles/30320500/Outreach", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000OutreachEfforts", "/ARSUserFiles/30320500/OutreachEfforts", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Photos", "/ARSUserFiles/30320500/Photos", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Products", "/ARSUserFiles/30320500/Products", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000ProductsServices", "/ARSUserFiles/30320500/ProductsServices", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Safety", "/ARSUserFiles/30320500/Safety", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000Sidney", "/ARSUserFiles/30320500/Sidney", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000SidneyInfo", "/ARSUserFiles/30320500/SidneyInfo", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360000TEAMTechTransfer", "/ARSUserFiles/30320500/TEAMTechTransfer", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360510PMRUPersonnel", "/ARSUserFiles/30320505/PMRUPersonnel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360510Quaranine", "/ARSUserFiles/30320505/Quaranine", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360510test", "/ARSUserFiles/30320505/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54360520ASRUPersonnel", "/ARSUserFiles/30320510/ASRUPersonnel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000Administrative", "/ARSUserFiles/30400500/Administrative", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000CattleHeatStress", "/ARSUserFiles/30400500/CattleHeatStress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000CattleResearch", "/ARSUserFiles/30400500/CattleResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000HeatStress", "/ARSUserFiles/30400500/HeatStress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000HeatStressHeatStress", "/ARSUserFiles/54380000HeatStress/HeatStressHeatStress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000SheepResearch", "/ARSUserFiles/30400500/SheepResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000SwineResearch", "/ARSUserFiles/30400500/SwineResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000Test", "/ARSUserFiles/30400500/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000UniversityofNebraskaLincoln", "/ARSUserFiles/30400500/UniversityofNebraskaLincoln", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380000Vacancies", "/ARSUserFiles/30400500/Vacancies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380500Internships", "/ARSUserFiles/30400500/Internships", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380540CattleNutrition", "/ARSUserFiles/54380540/CattleNutrition", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380540FeedlotWaste", "/ARSUserFiles/54380540/FeedlotWaste", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380560CattleHeatStress", "/ARSUserFiles/54380560/CattleHeatStress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380560HeatStress", "/ARSUserFiles/54380560/HeatStress", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54380560Test", "/ARSUserFiles/54380560/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54400000NewEmployeeResources", "/ARSUserFiles/30420000/NewEmployeeResources", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54402000HardWinterWheatRegionalNurseryProgram", "/ARSUserFiles/30421000/HardWinterWheatRegionalNurseryProgram", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54420000Sclerotinia", "/ARSUserFiles/30600500/Sclerotinia", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54420505WeilinShelver", "/ARSUserFiles/30600505/WeilinShelver", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54420515RogerALeopold", "/ARSUserFiles/30600510/RogerALeopold", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54450000Glomalin", "/ARSUserFiles/30640500/Glomalin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54450000test", "/ARSUserFiles/30640500/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54450000WaterShed", "/ARSUserFiles/30640500/WaterShed", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54452000CSC", "/ARSUserFiles/30640500/CSC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000EasternSDSoilWaterResearchFarm", "/ARSUserFiles/30800500/EasternSDSoilWaterResearchFarm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000EasternSDSoilWaterResearchFarmESDSWRF", "/ARSUserFiles/54470000EasternSDSoilWate/EasternSDSoilWaterResearchFarmESDSWRF", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000EasternSouthDakotaSoilWaterResearchFarm", "/ARSUserFiles/30800500/EasternSouthDakotaSoilWaterResearchFarm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000ESDSWRF", "/ARSUserFiles/30800500/ESDSWRF", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000IPRI", "/ARSUserFiles/30800500/IPRI", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000MidwestCarabids", "/ARSUserFiles/30800500/MidwestCarabids", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000NoTillWorkingGroup", "/ARSUserFiles/30800500/NoTillWorkingGroup", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000ResearchFarm", "/ARSUserFiles/30800500/ResearchFarm", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54470000ResearchontheCoccinellidae", "/ARSUserFiles/30800500/ResearchontheCoccinellidae", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000ArchivedNewsArticles", "/ARSUserFiles/30620500/ArchivedNewsArticles", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000Mission", "/ARSUserFiles/30620500/Mission", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News1997", "/ARSUserFiles/30620500/News1997", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News1998", "/ARSUserFiles/30620500/News1998", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News1999", "/ARSUserFiles/30620500/News1999", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2000", "/ARSUserFiles/30620500/News2000", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2001", "/ARSUserFiles/30620500/News2001", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2002", "/ARSUserFiles/30620500/News2002", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2003", "/ARSUserFiles/30620500/News2003", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2004", "/ARSUserFiles/30620500/News2004", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2005", "/ARSUserFiles/30620500/News2005", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2006", "/ARSUserFiles/30620500/News2006", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2006a", "/ARSUserFiles/30620500/News2006a", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2007", "/ARSUserFiles/30620500/News2007", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2008", "/ARSUserFiles/30620500/News2008", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2009", "/ARSUserFiles/30620500/News2009", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2010", "/ARSUserFiles/30620500/News2010", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2011", "/ARSUserFiles/30620500/News2011", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2012", "/ARSUserFiles/30620500/News2012", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2013", "/ARSUserFiles/30620500/News2013", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000News2014", "/ARSUserFiles/30620500/News2014", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000NutritionStudies", "/ARSUserFiles/30620500/NutritionStudies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000OldPublications", "/ARSUserFiles/30620500/OldPublications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/54500000ZincAbsorptionStudy42", "/ARSUserFiles/30620500/ZincAbsorptionStudy42", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/60663500SaseendranAnapalli", "/ARSUserFiles/60663500/SaseendranAnapalli", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/60800500CGC", "/ARSUserFiles/60800500/CGC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/60800500WRDG", "/ARSUserFiles/60800500/WRDG", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62020000SPAAdvisoryCouncilforOfficeProfessionals", "/ARSUserFiles/62024005/SPAAdvisoryCouncilforOfficeProfessionals", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62024005AerialApplicationResearch", "/ARSUserFiles/62024005/AerialApplicationResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62024005SPAAdvisoryCouncilforOfficeProfessionals", "/ARSUserFiles/62024005/SPAAdvisoryCouncilforOfficeProfessionals", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62050000KerrvilleWeatherData", "/ARSUserFiles/30940500/KerrvilleWeatherData", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62060000ClimaticDatafortheUS", "/ARSUserFiles/30980500/ClimaticDatafortheUS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62060000Hydrology", "/ARSUserFiles/30980500/Hydrology", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62060000Mintemp", "/ARSUserFiles/30980500/Mintemp", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62060000USClimaticDAta", "/ARSUserFiles/30980500/USClimaticDAta", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62060505MikeWhite", "/ARSUserFiles/62060505/MikeWhite", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62180500SouthernPlainsClimateHub", "/ARSUserFiles/30700500/SouthernPlainsClimateHub", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62180500USDASouthernPlainsClimateHub", "/ARSUserFiles/30700500/USDASouthernPlainsClimateHub", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62250000Admin", "/ARSUserFiles/60280000/Admin", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62250000HarryKDupree", "/ARSUserFiles/60280000/HarryKDupree", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62250000SPBLLocationAdministrativeOffice", "/ARSUserFiles/60280000/SPBLLocationAdministrativeOffice", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/622505005thInternationalRiceBlastConference", "/ARSUserFiles/60280500/5thInternationalRiceBlastConference", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62250500DBNrrcWeatherStation", "/ARSUserFiles/60280500/DBNrrcWeatherStation", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62250500GeneticStocksOryzaGSORCollection", "/ARSUserFiles/60280500/GeneticStocksOryzaGSORCollection", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62260000UnitHighlights", "/ARSUserFiles/60220500/UnitHighlights", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62351500NewsandEvents", "/ARSUserFiles/30501000/NewsandEvents", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/62351500PressReleases", "/ARSUserFiles/30501000/PressReleases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64000000Beth", "/ARSUserFiles/60000000/Beth", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64000000DeltaHumanNutritionResearch", "/ARSUserFiles/60000000/DeltaHumanNutritionResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64000000MidSouthAreaOfficeProfessionals", "/ARSUserFiles/60000000/MidSouthAreaOfficeProfessionals", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64000000MSAStatisticalServices", "/ARSUserFiles/60000000/MSAStatisticalServices", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/640210002010SummerWorkers", "/ARSUserFiles/60660500/2010SummerWorkers", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000AdiministrativeSupportStaff", "/ARSUserFiles/60660500/AdiministrativeSupportStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000AdministrativeSupportStaff", "/ARSUserFiles/60660500/AdministrativeSupportStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000ClintAllen", "/ARSUserFiles/60660500/ClintAllen", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000GuidelinesforSelectingSTEPEmployees", "/ARSUserFiles/60660500/GuidelinesforSelectingSTEPEmployees", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000index", "/ARSUserFiles/60660500/index", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000InnovativeControlofTarnishedPlantBug", "/ARSUserFiles/60660500/InnovativeControlofTarnishedPlantBug", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000InsectResistancetoBtToxins", "/ARSUserFiles/60660500/InsectResistancetoBtToxins", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000IRM", "/ARSUserFiles/60660500/IRM", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000LarryAdams", "/ARSUserFiles/60660500/LarryAdams", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000MeetingsPresentations", "/ARSUserFiles/60660500/MeetingsPresentations", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000People", "/ARSUserFiles/60660500/People", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000ResearchScienceStaff", "/ARSUserFiles/60660500/ResearchScienceStaff", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000ResistanceandInsectPestManagement", "/ARSUserFiles/60660500/ResistanceandInsectPestManagement", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000SIMRUAnnualReport", "/ARSUserFiles/60660500/SIMRUAnnualReport", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000SouthernInsectManagementUnitsSummerWorker", "/ARSUserFiles/60660500/SouthernInsectManagementUnitsSummerWorker", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000SpecialEvents", "/ARSUserFiles/60660500/SpecialEvents", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000SummerWorker", "/ARSUserFiles/60660500/SummerWorker", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000ToddUgine", "/ARSUserFiles/60660500/ToddUgine", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021000Visitors", "/ARSUserFiles/60660500/Visitors", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021500NationalCottonVarietyTest", "/ARSUserFiles/60661000/NationalCottonVarietyTest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64021500UniformSoybeanTestsSouthernStates", "/ARSUserFiles/60661000/UniformSoybeanTestsSouthernStates", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64022000SWSRUNews", "/ARSUserFiles/64022000/SWSRUNews", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500BrianPeterson", "/ARSUserFiles/60662000/BrianPeterson", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrBrianBosworth", "/ARSUserFiles/60662000/DrBrianBosworth", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrBrianPeterson", "/ARSUserFiles/60662000/DrBrianPeterson", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrBrianScheffler", "/ARSUserFiles/60662000/DrBrianScheffler", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrBrianSmall", "/ARSUserFiles/60662000/DrBrianSmall", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrGeoffWaldbieser", "/ARSUserFiles/60662000/DrGeoffWaldbieser", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrKennethBDavis", "/ARSUserFiles/60662000/DrKennethBDavis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrLanieBilodeau", "/ARSUserFiles/60662000/DrLanieBilodeau", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrLesTorrans", "/ARSUserFiles/60662000/DrLesTorrans", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrPaulZimba", "/ARSUserFiles/60662000/DrPaulZimba", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500DrPeterSilverstein", "/ARSUserFiles/60662000/DrPeterSilverstein", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500Genomic", "/ARSUserFiles/60662000/Genomic", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64023500test", "/ARSUserFiles/60662000/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64025500RuixiuSui", "/ARSUserFiles/60663500/RuixiuSui", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64040000DonnaMarshall", "/ARSUserFiles/60620500/DonnaMarshall", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64040000OrnamentalPlantsResearch", "/ARSUserFiles/60620500/OrnamentalPlantsResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64040000OrnamentalResearch", "/ARSUserFiles/60620500/OrnamentalResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64080000index", "/ARSUserFiles/60600000/index", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64080000NewsUpdates", "/ARSUserFiles/60600000/NewsUpdates", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64080000VAC", "/ARSUserFiles/60600000/VAC", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64080515FredRhoton", "/ARSUserFiles/60600510/FredRhoton", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64200500ConservationSystems", "/ARSUserFiles/60100500/ConservationSystems", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64355100NationalFormosanTermiteProgramCoordination", "/ARSUserFiles/64355100/NationalFormosanTermiteProgramCoordination", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64355100NationProgramCoordination", "/ARSUserFiles/64355100/NationProgramCoordination", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/64450000ResearchScientist", "/ARSUserFiles/50400500/ResearchScientist", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66000000Nickel", "/ARSUserFiles/66000000/Nickel", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66000000SAAACOP", "/ARSUserFiles/66000000/SAAACOP", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66000000SAAODEO", "/ARSUserFiles/66000000/SAAODEO", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66000000SAAOutreachDiversityandEqualOpportunity", "/ARSUserFiles/66000000/SAAOutreachDiversityandEqualOpportunity", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66000000SAASecretarialAdvisoryCouncil", "/ARSUserFiles/66000000/SAASecretarialAdvisoryCouncil", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/660700002010CuratorsWorkshop", "/ARSUserFiles/60460500/2010CuratorsWorkshop", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/660700002010CuratorWorkshop", "/ARSUserFiles/60460500/2010CuratorWorkshop", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/660700002010CuratorWorkshopAtlanta", "/ARSUserFiles/60460500/2010CuratorWorkshopAtlanta", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120500i", "/ARSUserFiles/60400500/i", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120500iiSHEMs", "/ARSUserFiles/66120500i/iiSHEMs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120500iSHEMs", "/ARSUserFiles/60401000/iSHEMs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120500Safetyandenviromentalpage", "/ARSUserFiles/60400500/Safetyandenviromentalpage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508NARMS", "/ARSUserFiles/60400520/NARMS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508NARMSInteractiveDataQueryPage", "/ARSUserFiles/60400520/NARMSInteractiveDataQueryPage", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508Test", "/ARSUserFiles/60400520/Test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508test2", "/ARSUserFiles/60400520/test2", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508test3", "/ARSUserFiles/60400520/test3", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120508Thisisatest", "/ARSUserFiles/60400520/Thisisatest", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700AvianInfluenza", "/ARSUserFiles/60401030/AvianInfluenza", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700Future", "/ARSUserFiles/60401030/Future", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700History", "/ARSUserFiles/60401030/History", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700MissionStatements", "/ARSUserFiles/60401030/MissionStatements", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700MucosalImmunityAndVaccination", "/ARSUserFiles/60401030/MucosalImmunityAndVaccination", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700NewcastleDisease", "/ARSUserFiles/60401030/NewcastleDisease", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700PEMS", "/ARSUserFiles/60401030/PEMS", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66120700SalmonellaEnteritidis", "/ARSUserFiles/60401030/SalmonellaEnteritidis", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66150000CMAVEHistorycs", "/ARSUserFiles/60360500/CMAVEHistorycs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66151000RiftValleyFever", "/ARSUserFiles/60360500/RiftValleyFever", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66151015FireAntandPestAntResearch", "/ARSUserFiles/60360510/FireAntandPestAntResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66151035ChemResearch", "/ARSUserFiles/60360520/ChemResearch", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66151035ResearchHighlights", "/ARSUserFiles/60360520/ResearchHighlights", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66180000Directions", "/ARSUserFiles/60340500/Directions", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66180000USHRLPublicationonCitrusGreeningDisease", "/ARSUserFiles/60340500/USHRLPublicationonCitrusGreeningDisease", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500AirQuality", "/ARSUserFiles/60701500/AirQuality", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500AirQualityAirQualityProject", "/ARSUserFiles/66452500AirQuality/AirQualityAirQualityProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500AirQualityProject", "/ARSUserFiles/60701500/AirQualityProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500asdf", "/ARSUserFiles/60701500/asdf", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500CerealImprovementandDiseases", "/ARSUserFiles/60701500/CerealImprovementandDiseases", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Climate", "/ARSUserFiles/60701500/Climate", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Downloads", "/ARSUserFiles/60701500/Downloads", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500GlobalChangeAirQualityProject", "/ARSUserFiles/60701500/GlobalChangeAirQualityProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Ozone", "/ARSUserFiles/60701500/Ozone", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500OzoneProject", "/ARSUserFiles/60701500/OzoneProject", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Papers", "/ARSUserFiles/60701500/Papers", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Plants", "/ARSUserFiles/60701500/Plants", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Project:AirQuality", "/ARSUserFiles/60701500/Project:AirQuality", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500Publications", "/ARSUserFiles/60701500/Publications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500PublicationsPublications", "/ARSUserFiles/66452500Publications/PublicationsPublications", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500ResearchSpotlight", "/ARSUserFiles/60701500/ResearchSpotlight", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500SmallGrainsGenotypingLaboratory", "/ARSUserFiles/60701500/SmallGrainsGenotypingLaboratory", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66452500test", "/ARSUserFiles/60701500/test", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66570000ForKidsOnly", "/ARSUserFiles/60820500/ForKidsOnly", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/66570000Ozone", "/ARSUserFiles/60820500/Ozone", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/80420510YunYang", "/ARSUserFiles/80420510/YunYang", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/80420525SensingTechnologies", "/ARSUserFiles/80420525/SensingTechnologies", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/cottontt", "/ARSUserFiles/00000000/cottontt", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/eggs", "/ARSUserFiles/80400530/eggs", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/m", "/ARSUserFiles/00000000/m", RegexOptions.IgnoreCase);
				redirectUrl = Regex.Replace(redirectUrl, @"/SP2UserFiles/ad_hoc/SANDwitch", "/ARSUserFiles/80400530/SANDwitch", RegexOptions.IgnoreCase);

				// Replace Old Mode Codes

				List<ModeCodeNew> modeCodeNewList = Helpers.Aris.ModeCodesNews.GetAllNewModeCode();

				if (modeCodeNewList != null && modeCodeNewList.Any())
				{
					foreach (ModeCodeNew modeCodeNewItem in modeCodeNewList)
					{
						if (redirectUrl.IndexOf("/ARSUserFiles/" + modeCodeNewItem.ModecodeOld + "/") >= 0)
						{
							redirectUrl = redirectUrl.Replace("/ARSUserFiles/" + modeCodeNewItem.ModecodeOld + "/", "/ARSUserFiles/" + modeCodeNewItem.ModecodeNew + "/");
						}
					}
				}
			}

			if (redirectUrl != null && redirectUrl.ToLower() == badUrl.ToLower())
			{
				redirectUrl = null;
			}

			return redirectUrl;
		}
	}

	public class RedirectToNode
	{
		public string OldUrl { get; set; }
		public int UmbracoId { get; set; }
		public string AppendString { get; set; }
		public string ManualUrl { get; set; }
	}
}
