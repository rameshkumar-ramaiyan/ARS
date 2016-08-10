using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportDocs.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using System.Xml;

using System.Data;

using USDA_ARS.LocationsWebApp.Models;
using System.Text.RegularExpressions;

using System.Data.SqlClient;

using ZetaHtmlCompressor;
using System.IO;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using System.Net;
using USDA_ARS.Umbraco.Extensions.Helpers;

namespace USDA_ARS.ImportDocs
{
   class Program
   {
      static string LOG_FILE_TEXT = "";
      static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
      static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
      static List<ModeCodeLookup> MODE_CODE_LIST = null;
      static List<PersonLookup> PEOPLE_LIST = null;
      static List<DocFolderLookup> DOC_FOLDER_ID_LIST = null;
      static List<SubSiteLookup> SUB_SITE_LIST = null;
      static List<AdHocFolderLookup> AD_HOC_FOLDER_LIST = new List<AdHocFolderLookup>();

      static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;


      static void Main(string[] args)
      {
         AddLog("-= IMPORT DOCS =-", LogFormat.Header);

         bool forceCacheUpdate = false;
         bool updateNonImportOnly = false;
         bool subsitesOnly = false;
         bool addHocOnly = false;
         bool personOnly = false;
         bool placeOnly = false;
         bool fixARSUserFiles = false;


         if (args != null && args.Length == 1)
         {
            forceCacheUpdate = Convert.ToBoolean(args[0]);
         }
         else if (args != null && args.Length == 2)
         {
            if (args[1] == "non-imported-only")
            {
               updateNonImportOnly = true;
            }
            else if (args[1] == "subsites-only")
            {
               subsitesOnly = true;
            }
            else if (args[1] == "adhoc-only")
            {
               addHocOnly = true;
            }
            else if (args[1] == "person-only")
            {
               personOnly = true;
            }
            else if (args[1] == "place-only")
            {
               placeOnly = true;
            }
            else if (args[1] == "fix-arsuserfiles")
            {
               fixARSUserFiles = true;
            }
         }

         AddLog("Getting People Sites From Umbraco...");
         GeneratePeopleList(forceCacheUpdate);
         AddLog("Done. Count: " + PEOPLE_LIST.Count, LogFormat.Success);
         AddLog("");

         AddLog("Getting Mode Codes From Umbraco...");
         GenerateModeCodeList(forceCacheUpdate);
         AddLog("Done. Count: " + MODE_CODE_LIST.Count, LogFormat.Success);
         AddLog("");

         AddLog("Getting Doc Folders From Umbraco...");
         GenerateDocFolderList(forceCacheUpdate);
         AddLog("Done. Count: " + DOC_FOLDER_ID_LIST.Count, LogFormat.Success);
         AddLog("");

         AddLog("Getting New Mode Codes...");
         MODE_CODE_NEW_LIST = GetNewModeCodesAll();
         AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count, LogFormat.Success);
         AddLog("");


         if (false == fixARSUserFiles)
         {
            AddLog("Importing Docs", LogFormat.Header);
            ImportDocs(updateNonImportOnly, addHocOnly, personOnly, placeOnly, subsitesOnly);
         }
         else
         {
            AddLog("Fix ARSUserFile Links", LogFormat.Header);
            FixArsUserFileLinks();
         }


      }


      static void ImportDocs(bool updateNonImportOnly = false, bool addHocOnly = false, bool personOnly = false, bool placeOnly = false, bool subsitesOnly = false)
      {
         // IMPORT SUB SITES

         AddLog("-= IMPORT DOCS =-");
         AddLog("");
         AddLog("");

         if (true == updateNonImportOnly)
         {
            // TODO: DOUBLE CHECK THESE...
            AddLog("Updating non-imported docs...");
            UpdateNonImportedPage("Research Home", "/Research/research.htm", 1104, "");
            UpdateNonImportedPage("National Programs", "/research/programs.htm", 1105, "");
            UpdateNonImportedPage("News & Events", "/News/News.htm", 29627, "");
            UpdateNonImportedPage("Careers at ARS Info", "/Careers/Careers.htm", 8031, "");
            UpdateNonImportedPage("Press Room", "/News/docs.htm?docid=1383", 8030, "");
            UpdateNonImportedPage("Briefing Room", "/News/docs.htm?docid=1281", 8003, "");
            UpdateNonImportedPage("Social Media Tools and Resources", "/News/Docs.htm?docid=23888", 131742, "");
            UpdateNonImportedPage("Image Gallery", "/News/Docs.htm?docid=23559", 1145, "");

            UpdateNonImportedPage("Manuscripts by Strategic Topical Areas", "/services/TTBrowse.htm", 8101, null, null, true);


         }
         else if (true == subsitesOnly)
         {
            AddLog("Importing Careers pages...");
            AddSubsitePages("Careers", 8058, 0, "careers"); //Umbraco Careers Node ID: 8058
            AddLog("");

            AddLog("Importing National Advisory Council for Office Professionals pages...");
            AddSubsitePages("HQsubsite", 130737, 21071, "NACOP");
            AddLog("");

            AddLog("Importing ARS Office of International Research Programs pages...");
            AddSubsitePages("irp", 130729, 1428, "OIRP");
            AddLog("");

            AddLog("Importing Office of Legislative Affiars...");
            AddSubsitePages("ARSLegisAffrs", 130738, 1332, "OLA");
            AddLog("");

            AddLog("Importing Office of Outreach, Diversity, and Equal Opportunity (ODEO) pages...");
            AddSubsitePages("odeo", 130739, 23071, "ODEO");
            AddLog("");

            AddLog("Importing Office of Scientific Quality Review (OSQR) pages...");
            AddSubsitePages("sciQualRev", 2133, 1286, "OSQR");
            AddLog("");

            AddLog("Importing CEAP pages...");
            AddSpecialAdHocPages("02020000StewardsCEAPsites", 130740, 18645, "CEAP");
            //AddSubsitePages("CEAP", 130740, 15358);
            AddLog("");
         }
         else if (true == addHocOnly || true == personOnly || true == placeOnly)
         {
            List<string> list = new List<string>();

            if (true == addHocOnly)
            {
               list.Add("ad_hoc");
            }
            else if (true == personOnly)
            {
               list.Add("person");
            }
            else if (true == placeOnly)
            {
               list.Add("Place");
            }

            for (int k = 0; k < list.Count; k++) // Loop through List with for
            {
               AddLog("Looping through: " + list[k]);
               AddLog("");

               DataTable dtAllDocumentIdsBasedOnDocTypeWithParam = new DataTable();
               AddLog("Getting docs based on doc type with parameter: " + list[k]);
               dtAllDocumentIdsBasedOnDocTypeWithParam = GetAllDocumentIdsBasedOnDocTypeWithParam(list[k]);

               int listCount = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows.Count;

               if (listCount > 0)
               {
                  DataView dvList = new DataView(dtAllDocumentIdsBasedOnDocTypeWithParam);

                  dvList.Sort = "OriginSite_ID";

                  dtAllDocumentIdsBasedOnDocTypeWithParam = dvList.ToTable();
               }

               for (int i = 0; i < dtAllDocumentIdsBasedOnDocTypeWithParam.Rows.Count; i++)
               {
                  try
                  {
                     AddLog(list[k] + " record: " + (i + 1) + " of " + listCount);
                     string title = "";
                     string currentversion = "";
                     string doctype = "";
                     string published = "";
                     string originSite_Type = "";
                     string originSite_ID = "";
                     bool displayTitle = false;
                     int docId = 0;

                     AddLog(" - Getting page info...");
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["title"] != null)
                     {
                        title = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["title"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["CurrentVersion_ID"] != null)
                     {
                        currentversion = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["CurrentVersion_ID"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["doctype"] != null)
                     {
                        doctype = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["doctype"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["published"] != null)
                     {
                        published = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["published"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["OriginSite_Type"] != null)
                     {
                        originSite_Type = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["OriginSite_Type"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["OriginSite_ID"] != null)
                     {
                        originSite_ID = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["OriginSite_ID"].ToString();
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["DisplayTitle"] != null)
                     {
                        displayTitle = Convert.ToBoolean(dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["DisplayTitle"]);
                     }
                     if (dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["DocId"] != null)
                     {
                        docId = Convert.ToInt32(dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i]["DocId"]);
                     }

                     if (true == string.IsNullOrWhiteSpace(title))
                     {
                        title = "[Missing Title]";
                     }

                     string adHocFolderName = string.Empty;
                     string htmlHeader = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>("HTMLHeader");
                     string keywords = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>("keywords");
                     if (list[k].ToString().Trim() == "ad_hoc")
                     {
                        adHocFolderName = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>("siteLabel").ToString();
                     }

                     AddLog(" - Doc ID: " + docId);
                     AddLog(" - Generating import page: " + title);

                     ImportPage newPage = GenerateImportPage(docId, currentversion, title, doctype, published, originSite_Type, originSite_ID, displayTitle, 
                           adHocFolderName, htmlHeader, keywords);

                     if (newPage != null)
                     {
                        if (list[k] != "person")
                        {
                           newPage.ParentSiteCode = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>("parent_Site_Code");
                        }

                        // PICK ONLY 1 OF THE 3 METHODS BELOW
                        if (list[k] == "Place")
                        {
                           // IS IT A PAGE FOR A MODE CODE?
                           AddDocToModeCode(originSite_ID, newPage);
                        }
                        else if (list[k] == "ad_hoc")
                        {
                           // IS IT A PAGE FOR A MODE CODE BUT ALSO HAS AN AD HOC?
                           AddDocToAdHoc(originSite_ID, adHocFolderName, newPage);
                        }
                        else if (list[k] == "person")
                        {
                           // OR IS IT A PAGE FOR A PERSON?
                           AddDocToPersonSite(Convert.ToInt32(originSite_ID), newPage);
                        }
                     }
                     else
                     {
                        AddLog("!! Page should not be imported (Doc ID: "+ docId + "): " + title);
                     }

                     AddLog("");
                  }
                  catch (Exception ex)
                  {
                     AddLog("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                     AddLog("ERROR: " + ex.ToString());
                  }
               }
            } // for (int k = 0; k < list.Count; k++)
         } // if (false == updateNonImportOnly)

         using (FileStream fs = File.Create("DOCS_LOG_FILE.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }


      static void FixArsUserFileLinks()
      {
         AddLog("");
         AddLog("Getting all Umbraco nodes with body text...");
         List<UmbracoPropertyData> fullDocsList = GetAllBodyTextData();

         int bodyTextUpdatedCount = 0;

         if (fullDocsList != null && fullDocsList.Any())
         {
            AddLog(" - Found: " + fullDocsList.Count, LogFormat.Success);
            AddLog("");

            int recordInc = 1;
            int recordCount = fullDocsList.Count;

            foreach (UmbracoPropertyData node in fullDocsList)
            {
               AddLog("Record " + recordInc + " / " + recordCount);
               AddLog("Umbraco Node (" + node.UmbracoId + "): " + node.Title, LogFormat.White);

               AddLog(" - Checking body text...");

               string bodyText = node.DataNtext;

               string bodyTextFixed = CleanHtml.CleanUpHtml(bodyText, null, MODE_CODE_NEW_LIST);

               if (bodyText != bodyTextFixed)
               {
                  AddLog(" - Change detected...");

                  //List<string> diff;
                  //IEnumerable<string> set1 = bodyText.Split(' ').Distinct();
                  //IEnumerable<string> set2 = bodyTextFixed.Split(' ').Distinct();

                  //if (set2.Count() > set1.Count())
                  //{
                  //   diff = set2.Except(set1).ToList();
                  //}
                  //else
                  //{
                  //   diff = set1.Except(set2).ToList();
                  //}

                  //AddLog(" - Changes:");
                  //foreach (string line in diff)
                  //{
                  //   AddLog(" - " + line, LogFormat.Info);
                  //}

                  UpdateUmbracoNode(node.UmbracoId, bodyTextFixed);

                  bodyTextUpdatedCount++;
               }
               else
               {
                  AddLog(" --- No Update Needed", LogFormat.Okay);
               }
               AddLog("");

               recordInc++;
            }

            AddLog("");
            AddLog("");
            AddLog("");
            AddLog(" - Umbraco nodes updated: " + bodyTextUpdatedCount, LogFormat.Info);
            AddLog("");
         }

         using (FileStream fs = File.Create("DOCS_LOG_FILE_ARS_FIX.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }


      static void AddSubsitePages(string subSite, int umbracoParentId, int mainDocId, string folderLabel)
      {
         DataTable subsiteDocs = new DataTable();
         // IMPORT CAREERS DOCS

         subsiteDocs = GetAllDocumentsBySubsite(subSite);

         if (subsiteDocs != null && subsiteDocs.Rows.Count > 0)
         {
            for (int i = 0; i < subsiteDocs.Rows.Count; i++)
            {
               try
               {
                  string title = subsiteDocs.Rows[i].Field<string>(0).ToString();
                  string currentversion = subsiteDocs.Rows[i].Field<int>(1).ToString();
                  string doctype = subsiteDocs.Rows[i].Field<string>(2).ToString();
                  string published = subsiteDocs.Rows[i].Field<string>(3).ToString();
                  string originSite_Type = subsiteDocs.Rows[i].Field<string>(4).ToString();
                  string originSite_ID = subsiteDocs.Rows[i].Field<string>(5).ToString();
                  bool displayTitle = subsiteDocs.Rows[i].Field<bool>(7);
                  int docId = subsiteDocs.Rows[i].Field<int>(8);
                  string htmlHeader = subsiteDocs.Rows[i].Field<string>("HTMLHeader");
                  string keywords = subsiteDocs.Rows[i].Field<string>("keywords");

                  ImportPage page = GenerateImportPage(docId, currentversion, title, doctype, published, originSite_Type, originSite_ID, displayTitle, "", htmlHeader, keywords);

                  if (docId == mainDocId)
                  {
                     UpdateSubsiteMainPage(subSite, umbracoParentId, page, folderLabel);
                  }
                  else
                  {
                     if (page != null)
                     {
                        AddDocToSubsite(subSite, umbracoParentId, page);
                     }
                  }

               }
               catch (Exception ex)
               {
                  AddLog("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                  AddLog("ERROR: " + ex.ToString());
               }
            }
         }
      }


      static void AddSpecialAdHocPages(string subSite, int umbracoParentId, int mainDocId, string folderLabel)
      {
         DataTable subsiteDocs = new DataTable();
         // IMPORT CAREERS DOCS

         subsiteDocs = GetAllDocumentsByAdHoc(subSite);

         if (subsiteDocs != null && subsiteDocs.Rows.Count > 0)
         {
            for (int i = 0; i < subsiteDocs.Rows.Count; i++)
            {
               try
               {
                  string title = subsiteDocs.Rows[i].Field<string>(0).ToString();
                  string currentversion = subsiteDocs.Rows[i].Field<int>(1).ToString();
                  string doctype = subsiteDocs.Rows[i].Field<string>(2).ToString();
                  string published = subsiteDocs.Rows[i].Field<string>(3).ToString();
                  string originSite_Type = subsiteDocs.Rows[i].Field<string>(4).ToString();
                  string originSite_ID = subsiteDocs.Rows[i].Field<string>(5).ToString();
                  bool displayTitle = subsiteDocs.Rows[i].Field<bool>(7);
                  int docId = subsiteDocs.Rows[i].Field<int>(8);
                  string htmlHeader = subsiteDocs.Rows[i].Field<string>("HTMLHeader");
                  string keywords = subsiteDocs.Rows[i].Field<string>("keywords");

                  ImportPage page = GenerateImportPage(docId, currentversion, title, doctype, published, originSite_Type, originSite_ID, displayTitle, "", htmlHeader, keywords);

                  if (docId == mainDocId)
                  {
                     UpdateSubsiteMainPage(subSite, umbracoParentId, page, folderLabel);
                  }
                  else
                  {
                     if (page != null)
                     {
                        AddDocToSubsite(subSite, umbracoParentId, page);
                     }
                  }

               }
               catch (Exception ex)
               {
                  AddLog("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                  AddLog("ERROR: " + ex.ToString());
               }
            }
         }
      }


      static void AddDocToModeCode(string modeCode, ImportPage importPage)
      {
         AddLog("Add doc to mode code...");
         DocFolderLookup getDocFolder = DOC_FOLDER_ID_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCode)).FirstOrDefault();

         //if (getDocFolder == null)
         //{
         //    ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode)).FirstOrDefault();

         //    if (modeCodeNew != null)
         //    {
         //        AddLog("Found Mode Code from old code: " + modeCode + " -> " + modeCodeNew.ModecodeNew);
         //        getDocFolder = DOC_FOLDER_ID_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew)).FirstOrDefault();
         //    }
         //}


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
               UpdateUmbracoNode(subNodeUmbracoId, importPage.BodyText, "/" + importPage.OldDocType + "/docs.htm?docid=" + importPage.OldDocId, importPage.OldDocId.ToString());
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


      static void AddDocToPersonSite(int personId, ImportPage importPage)
      {
         AddLog("Add doc to person site...");

         PersonLookup getPersonSite = PEOPLE_LIST.Where(p => p.PersonId == personId).FirstOrDefault();

         if (getPersonSite != null)
         {
            int umbracoParentId = getPersonSite.UmbracoPersonId;

            ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, 1);

            if (response != null && response.ContentList != null && response.ContentList.Any())
            {
               int umbracoId = response.ContentList[0].Id;

               AddLog(" - Page added:[Person Id: " + personId + "] (Umbraco Id: " + umbracoId + ") " + importPage.Title, LogFormat.Success);

               if (importPage.SubPages != null && importPage.SubPages.Any())
               {
                  foreach (ImportPage subPage in importPage.SubPages)
                  {
                     ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, subPage.PageNumber);

                     if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                     {
                        AddLog(" --- SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title, LogFormat.Success);
                     }
                     else
                     {
                        AddLog("!!ERROR SUBPAGE NOT ADDED!", LogFormat.Error);
                     }
                  }
               }
            }
            else
            {
               AddLog("!!ERROR SUBPAGE NOT ADDED!", LogFormat.Error);
            }
         }
         else
         {
            AddLog("!! PERSON CANNOT BE FOUND !! (" + personId + ")", LogFormat.Warning);
         }
      }


      static void AddDocToAdHoc(string modeCode, string adHocFolderName, ImportPage importPage)
      {
         AddLog("Add doc to mode code (" + importPage.ParentSiteCode + ") with ad hoc folder: " + adHocFolderName + "...");

         modeCode = importPage.ParentSiteCode;

         DocFolderLookup getDocFolder = DOC_FOLDER_ID_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCode)).FirstOrDefault();

         if (getDocFolder != null)
         {
            int umbracoParentId = getDocFolder.UmbracoDocFolderId;

            AdHocFolderLookup testAdHocFolder = AD_HOC_FOLDER_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCode) && p.AdHocFolderName.ToLower() == adHocFolderName.ToLower()).FirstOrDefault();

            if (testAdHocFolder == null)
            {
               ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode)).FirstOrDefault();

               if (modeCodeNew != null)
               {
                  AddLog("Found Mode Code from old code: " + modeCode + " -> " + modeCodeNew.ModecodeNew);
                  modeCode = modeCodeNew.ModecodeNew;

                  testAdHocFolder = AD_HOC_FOLDER_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew) && p.AdHocFolderName.ToLower() == adHocFolderName.ToLower()).FirstOrDefault();
               }

               modeCode = Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCode);

               if (testAdHocFolder != null)
               {
                  umbracoParentId = testAdHocFolder.UmbracoId;
               }
               else
               {
                  testAdHocFolder = AddAdHocFolder(modeCode, adHocFolderName, umbracoParentId);

                  if (testAdHocFolder != null)
                  {
                     umbracoParentId = testAdHocFolder.UmbracoId;

                     AD_HOC_FOLDER_LIST.Add(new AdHocFolderLookup() { ModeCode = modeCode, AdHocFolderName = adHocFolderName, UmbracoId = umbracoParentId });

                     AddLog(" - Ad Hoc Folder added:[Mode Code: " + modeCode + "] (Umbraco Parent Id: " + umbracoParentId + ") " + adHocFolderName);
                  }
               }
            }
            else
            {
               umbracoParentId = testAdHocFolder.UmbracoId;
            }


            if (umbracoParentId > 0)
            {
               ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, 1);

               if (response != null && response.ContentList != null && response.ContentList.Any())
               {
                  int umbracoId = response.ContentList[0].Id;

                  AddLog(" - Page added:[Mode Code: " + modeCode + "/" + adHocFolderName + "] (Umbraco Id: " + umbracoId + ") " + importPage.Title, LogFormat.Success);

                  if (importPage.SubPages != null && importPage.SubPages.Any())
                  {
                     foreach (ImportPage subPage in importPage.SubPages)
                     {
                        ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, subPage.PageNumber);

                        if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                        {
                           AddLog(" --- SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title, LogFormat.Success);
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
            else
            {
               AddLog("!!ERROR AD HOC FOLDER NOT ADDED!");
            }
         }
         else
         {
            AddLog(" - Old Mode Code... Not adding.");
         }
      }


      static void UpdateSubsiteMainPage(string subsite, int umbracoId, ImportPage importPage, string folderLabel)
      {
         AddLog("Updating doc to subsite main node: " + subsite + "...");

         ApiContent content = new ApiContent();

         string oldUrl = "";
         oldUrl = "/" + importPage.OldDocType + "/docs.htm?docid=" + importPage.OldDocId;

         content.Id = umbracoId;

         List<ApiProperty> properties = new List<ApiProperty>();

         string body = CleanHtml.CleanUpHtml(importPage.BodyText, "", MODE_CODE_NEW_LIST);

         properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
         properties.Add(new ApiProperty("oldId", importPage.OldDocId.ToString())); // Person's ID              
         properties.Add(new ApiProperty("oldUrl", oldUrl)); // current URL           
         properties.Add(new ApiProperty("folderLabel", folderLabel.ToLower()));

         content.Properties = properties;

         content.Save = 2;

         ApiRequest request = new ApiRequest();

         request.ContentList = new List<ApiContent>();
         request.ContentList.Add(content);
         request.ApiKey = API_KEY;

         ApiResponse responseBack = ApiCalls.PostData(request, "Post", 120000);

         if (responseBack != null && responseBack.Success)
         {
            AddLog(" --- Page Updated.");
         }
      }


      static void AddDocToSubsite(string subsite, int parentId, ImportPage importPage)
      {
         AddLog("Add doc to subsite: " + subsite + "...");

         if (parentId > 0)
         {
            ApiResponse response = AddUmbracoPage(parentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, 1, 2);

            if (response != null && response.ContentList != null && response.ContentList.Any())
            {
               int umbracoId = response.ContentList[0].Id;

               AddLog(" - Page added:[Sub Site: " + subsite + "] (Umbraco Id: " + umbracoId + ") " + importPage.Title, LogFormat.Success);

               if (importPage.SubPages != null && importPage.SubPages.Any())
               {
                  foreach (ImportPage subPage in importPage.SubPages)
                  {
                     ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, importPage.HtmlHeader, importPage.Keywords, subPage.PageNumber, 1, subsite);

                     if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                     {
                        AddLog(" --- SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title, LogFormat.Success);
                     }
                     else
                     {
                        AddLog("!!ERROR SUBPAGE NOT ADDED!");
                     }
                  }

                  PublishChildrenNodes(umbracoId);
               }
            }
            else
            {
               AddLog("!!ERROR SUBPAGE NOT ADDED!");
            }
         }
      }


      static AdHocFolderLookup AddAdHocFolder(string modeCode, string adHocFolderName, int umbracoParentId = 0)
      {
         AdHocFolderLookup newAdHocFolder = null;
         ModeCodeLookup getModeCode = null;

         if (umbracoParentId == 0 && false == string.IsNullOrEmpty(modeCode) && false == string.IsNullOrWhiteSpace(adHocFolderName))
         {
            getModeCode = MODE_CODE_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

            if (getModeCode != null)
            {
               if (umbracoParentId == 0)
               {
                  umbracoParentId = getModeCode.UmbracoId;
               }
            }
         }

         ApiResponse response = AddUmbracoFolder(umbracoParentId, adHocFolderName);

         if (response != null && response.ContentList != null && response.ContentList.Any())
         {
            newAdHocFolder = new AdHocFolderLookup();

            newAdHocFolder.ModeCode = modeCode;
            newAdHocFolder.AdHocFolderName = adHocFolderName;
            newAdHocFolder.UmbracoId = response.ContentList[0].Id;
         }

         return newAdHocFolder;
      }


      static ImportPage GenerateImportPage(int docId, string currentVersion, string title, string doctype, string published,
               string originSite_Type, string originSite_ID, bool displayTitle, string adHocFolderName, string htmlHeader, string keywords)
      {
         ImportPage newPage = new ImportPage();

         DataTable dtAllDocumentIdPagesBasedOnCurrentVersion = new DataTable();
         DataTable newDocpagesAfterDecryption = new DataTable();
         newDocpagesAfterDecryption.Columns.Add("DocPageNum");
         newDocpagesAfterDecryption.Columns.Add("EncDocPage");
         newDocpagesAfterDecryption.Columns.Add("CurrentVersion");
         newDocpagesAfterDecryption.Columns.Add("DecDocPage");
         DataTable newDocpagesAfterDecryption1 = new DataTable();
         //3. send to doc pages sp

         newPage.OldDocId = docId;
         newPage.Title = title;

         dtAllDocumentIdPagesBasedOnCurrentVersion = GetAllDocumentIdPagesBasedOnCurrentVersion(currentVersion);

         if (dtAllDocumentIdPagesBasedOnCurrentVersion.Rows.Count > 0)
         {
            AddLog(" - Found Doc [ID: " + docId + "][Site Type: " + originSite_Type + "][Site Id: " + originSite_ID + "]");
            AddLog(" - Title: " + title);

            // CHECK IF THE DOC HAS MUTLIPLE PAGES
            if (dtAllDocumentIdPagesBasedOnCurrentVersion.Rows.Count > 1)
            {
               // CREATE THE SUBPAGE OBJECT LIST
               newPage.SubPages = new List<ImportPage>();
            }

            for (int j = 0; j < dtAllDocumentIdPagesBasedOnCurrentVersion.Rows.Count; j++)
            {
               int docpageNum = dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<int>(0);

               string encString = ""; string decString = "";
               if (!string.IsNullOrEmpty(dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<string>(1)))
                  encString = dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<string>(1).ToString();
               else encString = string.Empty;
               string currentSubVersion = dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<int>(2).ToString();
               DataTable decStringtable = new DataTable();
               if (!string.IsNullOrEmpty(dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<string>(1)))
               {
                  if (encString.IndexOf("<") >= 0 || encString.IndexOf("the") >= 0 || encString.IndexOf("and") >= 0)
                  {
                     decString = encString;
                  }
                  else
                  {
                     decStringtable = GetAllRandomDocPagesDecrypted(encString);
                     decString = decStringtable.Rows[0].Field<string>(0);
                  }
               }
               else decString = "";

               newDocpagesAfterDecryption.Rows.Add(docpageNum, encString, currentSubVersion, decString);

               if (true == string.IsNullOrWhiteSpace(doctype))
               {
                  doctype = "Main";
               }

               // GET PAGE 1 (MAIN DOC)
               if (j == 0)
               {
                  AddLog(" - Adding main page: " + title);
                  newPage.OldDocId = docId; // Current SitePublisher Doc ID
                  newPage.Title = title.Trim(); // Document Title
                  newPage.DisableTitle = !displayTitle;
                  newPage.BodyText = decString.Trim(); // Document Body Text
                  newPage.OldDocType = doctype.Trim(); // SitePublisher Doc Type
                  newPage.HtmlHeader = htmlHeader != null ? htmlHeader.Trim() : "";
                  newPage.Keywords = keywords != null ? keywords.Trim() : "";
                  newPage.PageNumber = 1;
               }
               else
               {
                  AddLog(" - Adding sub page: " + docpageNum);
                  newPage.SubPages.Add(new ImportPage() { OldDocId = docId, PageNumber = docpageNum, BodyText = decString.Trim(), DisableTitle = !displayTitle });
               }
            }
         }
         else
         {
            newPage = null;
         }

         return newPage;
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

         body = CleanHtml.CleanUpHtml(body, "", MODE_CODE_NEW_LIST);

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

         properties.Add(new ApiProperty("pageHeaderScripts", CleanHtml.CleanUpHtml(htmlHeader, "", MODE_CODE_NEW_LIST))); // hide page title
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


      static ApiResponse AddUmbracoFolder(int parentId, string name)
      {
         ApiContent content = new ApiContent();

         content.Id = 0;
         content.Name = name;
         content.ParentId = parentId;
         content.DocType = "DocsFolder";
         content.Template = "";

         List<ApiProperty> properties = new List<ApiProperty>();

         content.Properties = properties;

         content.Save = 2;

         ApiRequest request = new ApiRequest();

         request.ContentList = new List<ApiContent>();
         request.ContentList.Add(content);
         request.ApiKey = API_KEY;

         ApiResponse responseBack = ApiCalls.PostData(request, "Post", 120000);

         return responseBack;
      }


      static void UpdateUmbracoNode(int umbracoId, string newBodyText, string oldUrl = "", string oldId = "")
      {
         AddLog(" - Updating node: " + umbracoId);

         ApiContent content = new ApiContent();

         content.Id = umbracoId;

         List<ApiProperty> properties = new List<ApiProperty>();

         properties.Add(new ApiProperty("bodyText", newBodyText)); // HTML of page

         if (false == string.IsNullOrEmpty(oldUrl))
         {
            properties.Add(new ApiProperty("oldUrl", oldUrl));
         }
         if (false == string.IsNullOrEmpty(oldId))
         {
            properties.Add(new ApiProperty("oldId", oldId));
         }

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


      static void PublishChildrenNodes(int parentId)
      {
         AddLog("Publishing nodes...");

         ApiRequest requestPublish = new ApiRequest();
         ApiContent contentPublish = new ApiContent();

         requestPublish.ApiKey = API_KEY;

         contentPublish.Id = parentId;

         requestPublish.ContentList = new List<ApiContent>();
         requestPublish.ContentList.Add(contentPublish);

         ApiResponse responseBack = ApiCalls.PostData(requestPublish, "PublishWithChildren", 620000);

         if (responseBack != null)
         {
            AddLog(" - Success: " + responseBack.Success);
            AddLog(" - Message: " + responseBack.Message);
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



      static List<DocFolderLookup> GetDocFoldersAll()
      {
         List<DocFolderLookup> docFoldersList = new List<DocFolderLookup>();

         if (MODE_CODE_LIST != null && MODE_CODE_LIST.Any())
         {
            DOC_FOLDER_ID_LIST = new List<DocFolderLookup>();

            foreach (ModeCodeLookup modeCodeNode in MODE_CODE_LIST)
            {
               ApiResponse responsePage = new ApiResponse();

               responsePage = GetCalls.GetNodeByUmbracoId(modeCodeNode.UmbracoId);

               if (responsePage != null && true == responsePage.Success && responsePage.ContentList != null && responsePage.ContentList.Count > 0)
               {
                  if (responsePage.ContentList[0].ChildContentList != null)
                  {
                     ApiContent docFolder = responsePage.ContentList[0].ChildContentList.Where(p => p.DocType == "DocsFolder").FirstOrDefault();

                     if (docFolder != null)
                     {
                        docFoldersList.Add(new DocFolderLookup { ModeCode = modeCodeNode.ModeCode, UmbracoDocFolderId = docFolder.Id });
                     }
                  }
               }
            }
         }


         return docFoldersList;
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


      static List<PersonLookup> GetPeopleAll()
      {
         List<PersonLookup> peopleList = new List<PersonLookup>();
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;

         ApiResponse responseBack = ApiCalls.PostData(request, "GetAllPeopleNodes");

         if (responseBack != null && responseBack.Success)
         {
            if (responseBack.ContentList != null && responseBack.ContentList.Any())
            {
               foreach (ApiContent node in responseBack.ContentList)
               {
                  if (node != null)
                  {
                     ApiProperty personLink = node.Properties.Where(p => p.Key == "personLink").FirstOrDefault();

                     if (personLink != null)
                     {
                        peopleList.Add(new PersonLookup { PersonId = Convert.ToInt32(personLink.Value), UmbracoPersonId = node.Id });

                        AddLog(" - Adding ModeCode (" + personLink.Value + "):" + node.Name);
                     }
                  }
               }
            }
         }

         return peopleList;
      }


      static List<DocFolderLookup> GetSubSitesAll()
      {
         List<DocFolderLookup> subSitesList = new List<DocFolderLookup>();

         subSitesList.Add(new DocFolderLookup { ModeCode = "02-00-00-00", UmbracoDocFolderId = 31703 });
         subSitesList.Add(new DocFolderLookup { ModeCode = "01-09-00-00", UmbracoDocFolderId = 2220 });
         //subSitesList.Add(new DocFolderLookup { ModeCode = "01-09-00-00", UmbracoDocFolderId = 2220 });

         return subSitesList;
      }


      static List<ModeCodeNew> GetNewModeCodesAll()
      {
         List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT * FROM NewModecodes";

         modeCodeNewList = db.Query<ModeCodeNew>(sql).ToList();

         return modeCodeNewList;
      }


      static List<UmbracoPropertyData> GetAllBodyTextData()
      {
         List<UmbracoPropertyData> umbPropertyDataList = null;

         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT cmsPropertyData.*, umbracoNode.uniqueID, umbracoNode.text FROM cmsPropertyData 
                           LEFT JOIN umbracoNode ON umbracoNode.id = cmsPropertyData.contentNodeId
                           WHERE NOT dataNtext IS NULL AND versionId IN
                           (SELECT versionId FROM cmsDocument WHERE published = 1) AND propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'bodytext')";

         umbPropertyDataList = db.Query<UmbracoPropertyData>(sql).ToList();

         return umbPropertyDataList;
      }


      static void UpdateNonImportedPage(string pageTitle, string url, int umbracoId, string oldId, string oldUrl = null, bool updateOldUrlOnly = false)
      {
         AddLog("Updating Non-Imported Page: " + pageTitle);
         ApiContent content = new ApiContent();

         content.Id = umbracoId;

         List<ApiProperty> properties = new List<ApiProperty>();
         string body = null;

         if (false == updateOldUrlOnly)
         {
            AddLog(" - Getting production body text...");
            body = GetProductionPage(url);
         }

         if (true == updateOldUrlOnly || false == string.IsNullOrEmpty(body))
         {
            AddLog(" - Done.");

            if (true == string.IsNullOrEmpty(oldUrl))
            {
               oldUrl = url;
            }

            if (false == updateOldUrlOnly)
            {
               properties.Add(new ApiProperty("bodyText", body));
            }
            properties.Add(new ApiProperty("oldUrl", oldUrl));
            properties.Add(new ApiProperty("oldId", oldId));

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            AddLog(" - Saving Page: " + pageTitle);

            ApiResponse responseBack = ApiCalls.PostData(request, "Post", 120000);

            if (responseBack != null && responseBack.Success)
            {
               AddLog(" --- Page Updated.");
            }
         }
         else
         {
            AddLog(" --- !Bypassing Page. No body text.");
         }
      }


      static string GetProductionPage(string url, bool usePreviewSite = true)
      {
         string output = "";
         string urlAddress = "";

         if (false == usePreviewSite)
         {
            urlAddress = "http://www.ars.usda.gov" + url;
         }
         else
         {
            urlAddress = "http://iapreview.ars.usda.gov" + url;
         }

         try
         {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
               Stream receiveStream = response.GetResponseStream();
               StreamReader readStream = null;

               if (response.CharacterSet == null)
               {
                  readStream = new StreamReader(receiveStream);
               }
               else
               {
                  readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
               }

               string html = readStream.ReadToEnd();

               response.Close();
               readStream.Close();


               if (false == string.IsNullOrEmpty(html))
               {
                  string between = null;

                  Match m2 = Regex.Match(html, @"(<!\-\- document content start \-\->)(.)*(<!\-\- document content end \-\->)", RegexOptions.Singleline);
                  if (m2.Success)
                  {
                     between = m2.Groups[0].Value;
                  }

                  if (false == string.IsNullOrEmpty(between))
                  {
                     output = between;

                     output = output.Replace("<font class=\"pageHeading\"></font>", "");

                     output = CleanHtml.CleanUpHtml(output, "", MODE_CODE_NEW_LIST);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            AddLog("!!! Can't get website page. !!!" + url);
         }

         return output;
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





      static DataTable GetAllDocumentIdsBasedOnDocTypeWithoutParam()
      {
         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllDocumentIdsBasedOnDocTypeWithoutParam]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            //sqlComm.Parameters.AddWithValue("@PersonId", personId);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }
      static DataTable GetAllDocumentIdsBasedOnDocTypeWithParam(string siteType)
      {
         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllDocumentIdsBasedOnDocTypeWithParam]";
         DataTable dt = new DataTable();

         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@SiteType", siteType);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }

      static DataTable GetAllDocumentsBySubsite(string subSite)
      {
         Locations locationsResponse = new Locations();
         string sql = @"SELECT
                        title,
                        CurrentVersion_ID,
                        doctype,
                        Published,
                        OriginSite_Type,
                        OriginSite_ID,
                        oldURL,
	                       DisplayTitle,
                        DocId,
                        HTMLHeader,
						                  keywords
                      FROM sitepublisherii.dbo.Documents

                      WHERE
                      published =  'p'
                      and 
                      sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
                      AND OriginSite_Type = 'SubSite' AND OriginSite_ID = @subSite
                      ORDER BY doctype, CurrentVersion_ID";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.Text;
            sqlComm.Parameters.AddWithValue("@subSite", subSite);

            DataSet ds = new DataSet();
            da.Fill(ds, "Docs");

            dt = ds.Tables["Docs"];

         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         return dt;
      }


      static DataTable GetAllDocumentsByAdHoc(string subSite)
      {
         Locations locationsResponse = new Locations();
         string sql = @"SELECT
                        title,
                        CurrentVersion_ID,
                        doctype,
                        Published,
                        OriginSite_Type,
                        OriginSite_ID,
                        oldURL,
	                       DisplayTitle,
                        DocId,
                        HTMLHeader,
						                  keywords
                      FROM sitepublisherii.dbo.Documents

                      WHERE
                      sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
                      AND OriginSite_Type = 'ad_hoc' AND OriginSite_ID = @subSite
                      ORDER BY doctype, CurrentVersion_ID";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.Text;
            sqlComm.Parameters.AddWithValue("@subSite", subSite);

            DataSet ds = new DataSet();
            da.Fill(ds, "Docs");

            dt = ds.Tables["Docs"];

         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         return dt;
      }


      static DataTable GetAllDocumentIdPagesBasedOnCurrentVersion(string currentversion)
      {
         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllDocumentIdPagesBasedOnCurrentVersion]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@CurrentVersionId", Convert.ToInt32(currentversion));

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }
      static DataTable GetAllRandomDocPagesDecrypted(string docPageEncrypted)
      {





         Locations locationsResponse = new Locations();
         string sql = "[uspGetAllNPDocPagesDecrypted]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@DocPageEncrypted", docPageEncrypted);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }


      static void GenerateDocFolderList(bool forceCacheUpdate)
      {
         DOC_FOLDER_ID_LIST = GetDocFolderCache();

         if (true == forceCacheUpdate || DOC_FOLDER_ID_LIST == null || DOC_FOLDER_ID_LIST.Count <= 0)
         {
            DOC_FOLDER_ID_LIST = CreateDocFolderCache();
         }

         DOC_FOLDER_ID_LIST.AddRange(GetSubSitesAll());
      }


      static void GenerateModeCodeList(bool forceCacheUpdate)
      {
         MODE_CODE_LIST = GetModeCodeLookupCache();

         if (true == forceCacheUpdate || MODE_CODE_LIST == null || MODE_CODE_LIST.Count <= 0)
         {
            MODE_CODE_LIST = CreateModeCodeLookupCache();
         }
      }


      static void GeneratePeopleList(bool forceCacheUpdate)
      {
         PEOPLE_LIST = GetPersonLookupCache();

         if (true == forceCacheUpdate || PEOPLE_LIST == null || PEOPLE_LIST.Count <= 0)
         {
            PEOPLE_LIST = CreatePersonLookupCache();
         }
      }


      static List<DocFolderLookup> CreateDocFolderCache()
      {
         List<DocFolderLookup> docFoldersList = new List<DocFolderLookup>();

         docFoldersList = GetDocFoldersAll();

         StringBuilder sb = new StringBuilder();

         if (docFoldersList != null)
         {
            foreach (DocFolderLookup docFolder in docFoldersList)
            {
               sb.AppendLine(docFolder.ModeCode + "|" + docFolder.UmbracoDocFolderId);
            }

            using (FileStream fs = File.Create("DOCS_doc-folder-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return docFoldersList;
      }


      static List<DocFolderLookup> GetDocFolderCache()
      {
         string filename = "DOCS_doc-folder-cache.txt";
         List<DocFolderLookup> docFoldersList = new List<DocFolderLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  docFoldersList.Add(new DocFolderLookup() { ModeCode = lineArray[0], UmbracoDocFolderId = Convert.ToInt32(lineArray[1]) });
               }
            }
         }

         return docFoldersList;
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

            using (FileStream fs = File.Create("DOCS_mode-code-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return modeCodeList;
      }


      static List<ModeCodeLookup> GetModeCodeLookupCache()
      {
         string filename = "DOCS_mode-code-cache.txt";
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


      static List<PersonLookup> CreatePersonLookupCache()
      {
         List<PersonLookup> personList = new List<PersonLookup>();

         personList = GetPeopleAll();

         StringBuilder sb = new StringBuilder();

         if (personList != null)
         {
            foreach (PersonLookup personItem in personList)
            {
               sb.AppendLine(personItem.PersonId + "|" + personItem.UmbracoPersonId);
            }

            using (FileStream fs = File.Create("DOCS_person-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return personList;
      }


      static List<PersonLookup> GetPersonLookupCache()
      {
         string filename = "DOCS_person-cache.txt";
         List<PersonLookup> personList = new List<PersonLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  personList.Add(new PersonLookup() { PersonId = Convert.ToInt32(lineArray[0]), UmbracoPersonId = Convert.ToInt32(lineArray[1]) });
               }
            }
         }

         return personList;
      }

   }
}
