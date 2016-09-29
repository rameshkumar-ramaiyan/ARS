using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportMultiDocs.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportMultiDocs
{
	class Program
	{
		static string LOG_FILE_TEXT = "";
		static string SP2ConnectionString = ConfigurationManager.ConnectionStrings["sitePublisherDbDSN"].ConnectionString;
		static string ARISConnectionString = ConfigurationManager.ConnectionStrings["arisPublicWebDbDSN"].ConnectionString;
		static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
		static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

		static List<ModeCodeLookup> MODE_CODE_LIST = null;
		static List<DocFolderLookup> DOC_FOLDER_ID_LIST = null;


		static void Main(string[] args)
		{

			AddLog("-= IMPORT DOCS =-", LogFormat.Header);

		}


		static void AddDocToModeCode(string modeCode, ImportPage importPage)
		{
			AddLog("Add doc to mode code...");
			DocFolderLookup getDocFolder = DOC_FOLDER_ID_LIST.Where(p => p.ModeCode == ModeCodes.ModeCodeAddDashes(modeCode)).FirstOrDefault();


			if (getDocFolder != null)
			{
				AddLog(" - Found Doc Folder: Umbraco Id: " + getDocFolder.UmbracoDocFolderId);

				int umbracoParentId = getDocFolder.UmbracoDocFolderId;

				int subNodeUmbracoId = 0;
				bool updateSubNode = false;

				if (importPage.OldDocType != null && importPage.Title != null)
				{
					if (importPage.OldDocType.ToLower() == "research" && importPage.Title.ToLower() == "index")
					{
						AddLog(" - Found Research index page.", LogFormat.Info);

						subNodeUmbracoId = GetNodeChildSubNode(modeCode, "SitesResearch");
					}
					else if (importPage.OldDocType.ToLower() == "careers" && importPage.Title.ToLower() == "index")
					{
						AddLog(" - Found Careers index page.", LogFormat.Info);

						subNodeUmbracoId = GetNodeChildSubNode(modeCode, "SitesCareers");
					}
					else if (importPage.OldDocType.ToLower() == "news" && importPage.Title.ToLower() == "index")
					{
						AddLog(" - Found News index page.", LogFormat.Info);

						subNodeUmbracoId = GetNodeChildSubNode(modeCode, "SitesNews");
					}
				}

				if (subNodeUmbracoId > 0)
				{
					AddLog(" - Updating Page for: " + importPage.OldDocType + "...");
					//UpdateUmbracoNode(subNodeUmbracoId, importPage.BodyText, "/" + importPage.OldDocType + "/docs.htm?docid=" + importPage.OldDocId, importPage.OldDocId.ToString());
					updateSubNode = true;
				}


				if (false == updateSubNode)
				{
					AddLog(" - Prepping document...");
					ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, 1);

					if (response != null && response.ContentList != null && response.ContentList.Any())
					{
						int umbracoId = response.ContentList[0].Id;

						AddLog(" - Page added:[Mode Code: " + modeCode + "] (Umbraco Id: " + umbracoId + ") " + importPage.Title, LogFormat.Success);

						if (importPage.SubPages != null && importPage.SubPages.Any())
						{
							foreach (ImportPage subPage in importPage.SubPages)
							{
								ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, subPage.PageNumber);

								if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
								{
									AddLog(" --- SubPage added:(Umbraco Id: " + subpageResponse.ContentList[0].Id + ") " + subPage.Title, LogFormat.Success);
								}
								else
								{
									AddLog("!!ERROR SUBPAGE NOT ADDED!");
								}
							}
						}
					}
					else
					{
						AddLog("!!ERROR SUBPAGE NOT ADDED!");
					}
				}
			}
			else
			{
				AddLog("!! CANNOT FIND MODE CODE!! (" + modeCode + ")");
			}
		}



		static int GetNodeChildSubNode(string modeCode, string docType)
		{
			int umbracoSubNodeId = 0;

			ApiContent subNode = null;

			ModeCodeLookup modeCodeLookup = MODE_CODE_LIST.Where(p => p.ModeCode == ModeCodes.ModeCodeAddDashes(modeCode)).FirstOrDefault();

			if (modeCodeLookup != null)
			{
				AddLog(" - Getting Sub Nodes for: " + modeCode);
				ApiResponse modeCodeNode = GetCalls.GetNodeByUmbracoId(modeCodeLookup.UmbracoId);

				if (modeCodeNode != null && modeCodeNode.ContentList != null && modeCodeNode.ContentList.Any() && modeCodeNode.ContentList[0].ChildContentList != null && modeCodeNode.ContentList[0].ChildContentList.Any())
				{
					AddLog(" - Looking for sub node: " + docType);

					subNode = modeCodeNode.ContentList[0].ChildContentList.Where(p => p.DocType == docType).FirstOrDefault();
				}
			}

			if (subNode != null)
			{
				AddLog(" - Sub node found for: " + docType + " | Umbraco Id: " + subNode.Id, LogFormat.Okay);
				umbracoSubNodeId = subNode.Id;
			}
			else
			{
				AddLog(" - !! Sub node not found for: " + docType, LogFormat.Warning);
			}

			return umbracoSubNodeId;
		}



		static ApiResponse AddUmbracoPage(int parentId, string name, string body, bool hidePageTitle, int oldId, string oldDocType, string htmlHeader, string keywords, int pageNum, int saveType = 2, string folderLabel = "")
		{
			ApiContent content = new ApiContent();

			string oldUrl = "";

			if (true == string.IsNullOrWhiteSpace(oldDocType))
			{
				oldDocType = "main";
			}

			oldUrl = "/" + oldDocType + "/docs.htm?docid=" + oldId;

			if (pageNum > 1)
			{
				oldUrl += "&page=" + pageNum;
				name = "Page " + pageNum;
			}


			if (true == string.IsNullOrEmpty(name))
			{
				name = "(Missing Title)";
			}
			else if (name.Trim().ToLower() == "index")
			{
				name = oldDocType;
			}

			if (true == string.IsNullOrEmpty(body))
			{
				body = "";
			}

			if (true == string.IsNullOrEmpty(oldUrl))
			{
				oldUrl = "";
			}

			content.Id = 0;
			content.Name = name;
			content.ParentId = parentId;
			content.DocType = "SiteStandardWebpage";
			content.Template = "StandardWebpage";

			List<ApiProperty> properties = new List<ApiProperty>();

			body = CleanHtml.CleanUpHtml(body, "", null);

			properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
			properties.Add(new ApiProperty("oldId", oldId.ToString())); // Person's ID              
			properties.Add(new ApiProperty("oldUrl", oldUrl)); // current URL           
			properties.Add(new ApiProperty("hidePageTitle", hidePageTitle)); // hide page title

			if (true == string.IsNullOrWhiteSpace(htmlHeader))
			{
				htmlHeader = "";
			}
			else
			{
				AddLog(" - Adding HTML header script...");
			}
			if (true == string.IsNullOrWhiteSpace(keywords))
			{
				keywords = "";
			}
			else
			{
				AddLog(" - Adding keywords...");
			}

			properties.Add(new ApiProperty("pageHeaderScripts", CleanHtml.CleanUpHtml(htmlHeader, "", null))); // hide page title
			properties.Add(new ApiProperty("keywords", keywords)); // hide page title

			if (false == string.IsNullOrEmpty(folderLabel))
			{
				properties.Add(new ApiProperty("folderLabel", folderLabel)); // hide page title
			}

			content.Properties = properties;

			content.Save = saveType;

			ApiRequest request = new ApiRequest();

			request.ContentList = new List<ApiContent>();
			request.ContentList.Add(content);
			request.ApiKey = API_KEY;

			ApiResponse responseBack = ApiCalls.PostData(request, "Post", 120000);

			return responseBack;
		}





		static void AddLog(string line, LogFormat logFormat = LogFormat.Normal)
		{
			Debug.WriteLine(line);

			if (logFormat == LogFormat.Normal)
			{
				Console.ResetColor();
			}
			else if (logFormat == LogFormat.Header)
			{
				Console.BackgroundColor = ConsoleColor.DarkBlue;
				Console.ForegroundColor = ConsoleColor.White;
			}
			else if (logFormat == LogFormat.Success)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Green;
			}
			else if (logFormat == LogFormat.Okay)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.DarkGreen;
			}
			else if (logFormat == LogFormat.Warning)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Yellow;
			}
			else if (logFormat == LogFormat.Error)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Red;
			}
			else if (logFormat == LogFormat.ErrorBad)
			{
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.DarkRed;
			}
			else if (logFormat == LogFormat.Info)
			{
				Console.BackgroundColor = ConsoleColor.Cyan;
				Console.ForegroundColor = ConsoleColor.DarkBlue;
			}
			else if (logFormat == LogFormat.Gray)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.DarkGray;
			}
			else if (logFormat == LogFormat.White)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.White;
			}

			Console.WriteLine(line);
			Console.ResetColor();
			LOG_FILE_TEXT += line + "\r\n";
		}



		enum LogFormat
		{
			Normal,
			Header,
			Success,
			Okay,
			Warning,
			Error,
			ErrorBad,
			Info,
			Gray,
			White
		}
	}
}
