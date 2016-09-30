using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportMultidocs.Models;
using USDA_ARS.ImportMultiDocs.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportMultiDocs
{
	class Program
	{
		static string LOG_FILE_TEXT = "";
		static string SP2ConnectionString = ConfigurationManager.ConnectionStrings["sitePublisherDbDSN"].ConnectionString;
		static string ARISConnectionString = ConfigurationManager.ConnectionStrings["arisPublicWebDbDSN"].ConnectionString;
		static string UmbracoConnectionString = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;
		static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
		static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

		static List<ModeCodeLookup> MODE_CODE_LIST = null;
		static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

		static void Main(string[] args)
		{

			AddLog("-= IMPORT DOCS =-", LogFormat.Header);


			AddLog("Getting Mode Codes From Umbraco...");
			GenerateModeCodeList(false);
			AddLog("Done. Count: " + MODE_CODE_LIST.Count, LogFormat.Success);
			AddLog("");


			AddLog("Getting New Mode Codes...");
			MODE_CODE_NEW_LIST = GetNewModeCodesAll();
			AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count, LogFormat.Success);
			AddLog("");



			AddLog("Getting Multi-Page Docs...");


			List<int> docIdList = GetMultiPageDocsTextFile();

			AddLog("Main Document count with multi pages: " + docIdList.Count);

			if (docIdList != null && docIdList.Any())
			{
				foreach (int docId in docIdList)
				{
					List<Sp2Document> docPageList = GetDocumentPagesByDocId(docId);

					if (docPageList != null && docPageList.Any())
					{
						AddLog("Title: " + docPageList[0].Title);
						AddLog("DocType: " + docPageList[0].DocType);
						AddLog("Origin Type: " + docPageList[0].OriginSiteType);
						AddLog("Origin ID: " + docPageList[0].OriginSiteId);

						int umbracoParentId = 0;


						string umbracoFolderDocTypeName = "";

						if (docPageList[0].DocType.ToLower() == "research")
						{
							umbracoFolderDocTypeName = "SitesResearch";
						}
						else if (docPageList[0].DocType.ToLower() == "careers")
						{
							umbracoFolderDocTypeName = "SitesCareers";
						}
						else if (docPageList[0].DocType.ToLower() == "news")
						{
							umbracoFolderDocTypeName = "SitesNews";
						}
						else
						{
							throw new Exception("Invalid Doc Type: " + docPageList[0].DocType);
						}

						if (docPageList[0].OriginSiteId.Length > 8)
						{
							docPageList[0].OriginSiteId = docPageList[0].OriginSiteId.Substring(0, 8);
						}

						string pageModeCode = ModeCodes.ModeCodeAddDashes(docPageList[0].OriginSiteId);


						ModeCodeLookup modeCodeLookup = MODE_CODE_LIST.Where(p => p.ModeCode == pageModeCode).FirstOrDefault();

						if (modeCodeLookup == null)
						{
							AddLog("Unable to find Mode Code [" + pageModeCode + "]. Trying old Mode Code...", LogFormat.Warning);

							ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == pageModeCode).FirstOrDefault();

							if (modeCodeNew != null)
							{
								pageModeCode = modeCodeNew.ModecodeNew;

								AddLog("New Mode Code Found: " + pageModeCode, LogFormat.Success);
							}
							else
							{
								AddLog("Could not find old Mode Code [" + pageModeCode + "]. Bypassing page...", LogFormat.Error);
								pageModeCode = "";
								AddLog("");
							}
						}

						if (false == string.IsNullOrEmpty(pageModeCode))
						{
							int umbracoFirstPageId = 0;
							umbracoParentId = GetNodeChildSubNode(pageModeCode, umbracoFolderDocTypeName);

							AddLog("Umbraco Folder: " + umbracoFolderDocTypeName + " | Umbraco ID: " + umbracoParentId);

							if (umbracoParentId > 0)
							{
								docPageList = docPageList.OrderBy(p => p.DocPage).ToList();

                        foreach (Sp2Document document in docPageList)
								{
									AddLog("- Doc ID: " + document.DocId);
									AddLog("- Title: " + document.Title, LogFormat.Okay);
									AddLog("- Page #: " + document.DocPage);

									if (document.DocPage == 1)
									{
										// save to umbracoParentId, get umbracoFirstPageId
										ApiResponse response = AddUmbracoPage(umbracoParentId, document.Title, document.BodyText, document.DisplayTitle ? false : true, document.DocId, document.DocType, document.HtmlHeader, document.Keywords, document.DocPage);

										if (response != null && response.ContentList != null && response.ContentList.Any())
										{
											umbracoFirstPageId = response.ContentList[0].Id;

											AddLog(" - Page (" + document.DocPage + ") added:[Mode Code: " + pageModeCode + "] (Umbraco Id: " + umbracoFirstPageId + ") " + document.Title, LogFormat.Success);

											AddLog(" - Adding redirect to imported page on main " + document.DocType + " page...");

											UpdateUmbracoNode(umbracoParentId, umbracoFirstPageId);
                              }
										else
										{
											AddLog("Doc ID: " + document.DocId + " - COULD NOT SAVE PAGE.", LogFormat.ErrorBad);
										}
									}
									else
									{
										if (umbracoFirstPageId > 0)
										{
											// Save all pages under umbracoFirstPageId
											ApiResponse response = AddUmbracoPage(umbracoFirstPageId, document.Title, document.BodyText, document.DisplayTitle ? false : true, document.DocId, document.DocType, document.HtmlHeader, document.Keywords, document.DocPage);

											if (response != null && response.ContentList != null && response.ContentList.Any())
											{
												AddLog(" - Page (" + document.DocPage + ") added:[Mode Code: " + pageModeCode + "] (Umbraco Id: " + response.ContentList[0].Id + ") " + document.Title, LogFormat.Success);
											}
											else
											{
												AddLog("Doc ID: " + document.DocId + " - COULD NOT SAVE PAGE.", LogFormat.ErrorBad);
											}
										}
										else
										{
											AddLog("Doc ID: " + document.DocId + " - First page did not save. Can't save sub-pages.", LogFormat.Error);
										}
									}
								}
							}
							else
							{
								AddLog("Unable to find the Umbraco Sub Folder (ex: Research/News/Careers)", LogFormat.Error);
							}
						}
					}

					AddLog("");
					AddLog("");
				}
			}
		}



		static List<int> GetMultiPageDocsTextFile()
		{
			string filename = "MULTI_PAGE_DOCS_LOAD_FILE.txt";
			List<int> docIdsList = new List<int>();

			if (true == File.Exists(filename))
			{
				using (StreamReader sr = File.OpenText(filename))
				{
					string s = "";
					while ((s = sr.ReadLine()) != null)
					{
						if (false == string.IsNullOrWhiteSpace(s))
						{
							docIdsList.Add(Convert.ToInt32(s.Trim()));
						}
					}
				}
			}

			return docIdsList;
		}



		static List<int> GetMultiPageDocs()
		{
			List<int> docIdList = new List<int>();
			Locations locationsResponse = new Locations();
			string sql = "[uspgetAllDistinctDocIdsWithMultiplePages]";
			DataTable dt = new DataTable();
			SqlConnection conn = new SqlConnection(ARISConnectionString);

			try
			{
				SqlDataAdapter da = new SqlDataAdapter();
				SqlCommand sqlComm = new SqlCommand(sql, conn);
				sqlComm.CommandTimeout = 300;

				da.SelectCommand = sqlComm;
				da.SelectCommand.CommandType = CommandType.StoredProcedure;

				DataSet ds = new DataSet();
				da.Fill(ds, "TheTable");

				dt = ds.Tables["TheTable"];

				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						docIdList.Add(Convert.ToInt32(dr[0]));

						AddLog("Found Doc ID: " + dr[0]);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				conn.Close();
			}

			return docIdList;
		}






		static List<Sp2Document> GetDocumentPagesByDocId(int docId)
		{
			List<Sp2Document> docPagesList = new List<Sp2Document>();
			Locations locationsResponse = new Locations();
			string sql = "SELECT * FROM Documents WHERE DocId = " + docId;
			DataTable dt = new DataTable();
			SqlConnection conn = new SqlConnection(SP2ConnectionString);

			try
			{
				SqlDataAdapter da = new SqlDataAdapter();
				SqlCommand sqlComm = new SqlCommand(sql, conn);
				sqlComm.CommandTimeout = 100;

				da.SelectCommand = sqlComm;
				da.SelectCommand.CommandType = CommandType.Text;

				DataSet ds = new DataSet();
				da.Fill(ds, "TheTable");

				dt = ds.Tables["TheTable"];

				if (dt.Rows.Count > 0)
				{
					docPagesList = GetDocPagesById(docId);

					if (docPagesList != null && docPagesList.Any())
					{
						for (int i = 0; i < docPagesList.Count; i++)
						{
							DataRow dr = dt.Rows[0];

							docPagesList[i].DisplayTitle = Convert.ToInt32(dr["DisplayTitle"]) == 1 ? true : false;
							docPagesList[i].OriginSiteType = dr["OriginSite_Type"].ToString().Trim();
							docPagesList[i].OriginSiteId = dr["OriginSite_ID"].ToString().Trim();
							docPagesList[i].Keywords = dr["keywords"].ToString().Trim();

							if (false == string.IsNullOrWhiteSpace(docPagesList[i].Keywords))
							{
								docPagesList[i].Keywords = docPagesList[i].Keywords.Replace("\r\n", ",");
							}

							docPagesList[i].HtmlHeader = dr["HTMLHeader"].ToString().Trim();
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				conn.Close();
			}

			return docPagesList;
		}



		static List<Sp2Document> GetDocPagesById(int docId)
		{
			List<Sp2Document> docsList = new List<Sp2Document>();
			Locations locationsResponse = new Locations();
			string sql = "[uspgetAllDocumentsBasedOnRandomDocIds]";
			DataTable dt = new DataTable();
			SqlConnection conn = new SqlConnection(ARISConnectionString);

			try
			{
				SqlDataAdapter da = new SqlDataAdapter();
				SqlCommand sqlComm = new SqlCommand(sql, conn);
				sqlComm.CommandTimeout = 100;

				da.SelectCommand = sqlComm;
				da.SelectCommand.CommandType = CommandType.StoredProcedure;
				sqlComm.Parameters.AddWithValue("@RandomDocId", docId);

				DataSet ds = new DataSet();
				da.Fill(ds, "TheTable");

				dt = ds.Tables["TheTable"];

				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						Sp2Document doc = new Sp2Document();

						doc.DocId = Convert.ToInt32(dr["docId"]);
						doc.Title = dr["docpagetitle"].ToString().Trim();
						doc.DocPage = Convert.ToInt32(dr["docpagenum"]);
						doc.DocType = dr["DocType"].ToString().Trim();
						doc.OriginSiteId = dr["OriginSite_ID"].ToString().Trim();
						doc.BodyText = dr["DocPageDecrypted"].ToString();

						if (false == string.IsNullOrWhiteSpace(doc.BodyText))
						{
							doc.BodyText = CleanHtml.CleanUpHtml(doc.BodyText, "", MODE_CODE_NEW_LIST);
						}

						doc.Keywords = "";
						doc.HtmlHeader = "";
						doc.DisplayTitle = true;

						docsList.Add(doc);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				conn.Close();
			}

			return docsList;
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





		static ApiResponse AddUmbracoPage(int parentId, string name, string body, bool hidePageTitle, int oldId, string oldDocType, string htmlHeader, string keywords, int pageNum, int saveType = 2, string folderLabel = "", string navCategory = "")
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
			}
			else
			{
				oldUrl += ",\r\n/" + oldDocType + "/docs.htm?docid=" + oldId + "&page=1";
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



			properties.Add(new ApiProperty("navigationCategory", navCategory)); // Navigation Category

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


		static void UpdateUmbracoNode(int umbracoId, int redirectId)
		{
			AddLog(" - Updating node: " + umbracoId);

			ApiContent content = new ApiContent();

			content.Id = umbracoId;

			List<ApiProperty> properties = new List<ApiProperty>();

			properties.Add(new ApiProperty("umbracoRedirect", redirectId)); // redirect umbraco id

			content.Properties = properties;

			content.Save = 2;

			ApiRequest request = new ApiRequest();

			request.ContentList = new List<ApiContent>();
			request.ContentList.Add(content);
			request.ApiKey = API_KEY;

			ApiResponse responseBack = ApiCalls.PostData(request, "Post", 120000);

			if (responseBack != null && responseBack.Success && responseBack.ContentList != null && responseBack.ContentList.Any())
			{
				if (responseBack.ContentList[0].Success)
				{
					AddLog(" --- Page Updated and Published.", LogFormat.Success);
					AddLog(" --- URL: " + responseBack.ContentList[0].Url, LogFormat.White);
				}
				else
				{
					AddLog(" --- !! ERROR: " + responseBack.ContentList[0].Message, LogFormat.Error);
				}
			}
			else
			{
				AddLog(" --- !! API ERROR: null", LogFormat.Error);
			}
		}


		static void GenerateModeCodeList(bool forceCacheUpdate)
		{
			MODE_CODE_LIST = GetModeCodeLookupCache();

			if (true == forceCacheUpdate || MODE_CODE_LIST == null || MODE_CODE_LIST.Count <= 0)
			{
				MODE_CODE_LIST = CreateModeCodeLookupCache();
			}
		}


		static List<ModeCodeLookup> CreateModeCodeLookupCache()
		{
			List<ModeCodeLookup> modeCodeList = new List<ModeCodeLookup>();

			modeCodeList = GetModeCodesAll();

			StringBuilder sb = new StringBuilder();

			if (modeCodeList != null)
			{
				foreach (ModeCodeLookup modeCodeItem in modeCodeList)
				{
					sb.AppendLine(modeCodeItem.ModeCode + "|" + modeCodeItem.UmbracoId + "|" + modeCodeItem.Url);
				}

				using (FileStream fs = File.Create("MULTI_DOCS_mode-code-cache.txt"))
				{
					// Add some text to file
					Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
					fs.Write(fileText, 0, fileText.Length);
				}
			}

			return modeCodeList;
		}


		static List<ModeCodeLookup> GetModeCodesAll()
		{
			List<ModeCodeLookup> modeCodeList = new List<ModeCodeLookup>();
			ApiRequest request = new ApiRequest();

			request.ApiKey = API_KEY;

			ApiResponse responseBack = ApiCalls.PostData(request, "GetAllModeCodeNodes");

			if (responseBack != null && responseBack.Success)
			{
				if (responseBack.ContentList != null && responseBack.ContentList.Any())
				{
					foreach (ApiContent node in responseBack.ContentList)
					{
						if (node != null)
						{
							ApiProperty modeCode = node.Properties.Where(p => p.Key == "modeCode").FirstOrDefault();

							if (modeCode != null)
							{
								modeCodeList.Add(new ModeCodeLookup { ModeCode = modeCode.Value.ToString(), UmbracoId = node.Id, Url = node.Url });

								AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
							}
						}
					}
				}
			}

			return modeCodeList;
		}


		static List<ModeCodeLookup> GetModeCodeLookupCache()
		{
			string filename = "MULTI_DOCS_mode-code-cache.txt";
			List<ModeCodeLookup> modeCodeList = new List<ModeCodeLookup>();

			if (true == File.Exists(filename))
			{
				using (StreamReader sr = File.OpenText(filename))
				{
					string s = "";
					while ((s = sr.ReadLine()) != null)
					{
						string[] lineArray = s.Split('|');

						modeCodeList.Add(new ModeCodeLookup() { ModeCode = lineArray[0], UmbracoId = Convert.ToInt32(lineArray[1]), Url = lineArray[2] });
					}
				}
			}

			return modeCodeList;
		}


		static List<ModeCodeNew> GetNewModeCodesAll()
		{
			List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

			Locations locationsResponse = new Locations();
			string sql = @"SELECT contentNodeId, dataNvarchar FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'oldModeCodes')
                            AND NOT dataNvarchar IS NULL AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";
			DataTable dt = new DataTable();
			SqlConnection conn = new SqlConnection(UmbracoConnectionString);

			try
			{
				SqlDataAdapter da = new SqlDataAdapter();
				SqlCommand sqlComm = new SqlCommand(sql, conn);
				sqlComm.CommandTimeout = 100;

				da.SelectCommand = sqlComm;
				da.SelectCommand.CommandType = CommandType.Text;

				DataSet ds = new DataSet();
				da.Fill(ds, "TheTable");

				dt = ds.Tables["TheTable"];

				if (dt.Rows.Count > 0)
				{

					foreach (DataRow dr in dt.Rows)
					{
						string oldModeCodeStr = dr["dataNvarchar"].ToString();

						List<string> oldModeCodeArray = oldModeCodeStr.Split(',').ToList();

						if (oldModeCodeArray != null && oldModeCodeArray.Any())
						{
							int umbracoId = Convert.ToInt32(dr["contentNodeId"]);

							ModeCodeLookup modeCodeLookup = MODE_CODE_LIST.Where(p => p.UmbracoId == umbracoId).FirstOrDefault();

							if (modeCodeLookup != null)
							{
								foreach (string oldModeCode in oldModeCodeArray)
								{
									ModeCodeNew modeCodeNew = new ModeCodeNew();

									modeCodeNew.ModecodeNew = modeCodeLookup.ModeCode;
									modeCodeNew.ModecodeOld = oldModeCode;

									modeCodeNewList.Add(modeCodeNew);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				conn.Close();
			}

			return modeCodeNewList;
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
