using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using USDA_ARS.ImportNavigation.Models;
using USDA_ARS.ImportNavigation.Objects;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportNavigation
{
   class Program
   {
      static string LOG_FILE_TEXT = "";
      static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
      static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

      static List<ModeCodeLookup> MODE_CODE_LIST = null;
      static List<ProgramLookup> PROGRAM_LIST = null;
      static List<ModeCodeFolderLookup> MODE_CODE_FOLDER_LIST = null;

      static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

      static List<UmbracoDocLookup> UMBRACO_DOC_LOOKUP = null;
      static List<PersonLookup> UMBRACO_PERSON_LOOKUP = null;
      static List<UmbracoDocLookup> UMBRACO_OLD_URL_LOOKUP = null;
      static List<SubsiteLookup> UMBRACO_SUBSITES_LOOKUP = null;


      static List<SubSite> VALID_SITES = null;
      static List<Document> VALID_DOCS = null;

      static List<ImportedNav> IMPORTED_NAV = new List<ImportedNav>();

      static DateTime TIME_STARTED = DateTime.MinValue;
      static DateTime TIME_ENDED = DateTime.MinValue;

      static int StartRecord = 0;

      static void Main(string[] args)
      {
         bool runImportNav = false;
         bool runLinkNav = false;
         bool runFixNav = false;

         if (args != null && args.Length >= 1)
         {
            if (args[0] == "import-nav")
            {
               runImportNav = true;
            }

            else if (args[0] == "link-nav")
            {
               runLinkNav = true;
            }

            else if (args[0] == "fix-nav")
            {
               runFixNav = true;
            }

            if (args.Length == 2)
            {
               StartRecord = Convert.ToInt32(args[1]);
            }
         }

         TIME_STARTED = DateTime.Now;

         AddLog("-= IMPORTING NAVIGATION =-", LogFormat.Header);
         AddLog("");
         AddLog("");

         AddLog("Testing database connection strings...");
         AddLog("");
         bool testSuccess = TestDbConnections();
         AddLog("Done.");
         AddLog("");
         AddLog("");

         if (true == testSuccess)
         {
            AddLog("Getting Mode Codes From Umbraco (~10 mins)...");
            GenerateModeCodeList(false);
            AddLog("Done. Count: " + MODE_CODE_LIST.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting National Programs From Umbraco...");
            GenerateProgramList(false);
            AddLog("Done. Count:" + PROGRAM_LIST.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Umbraco People (~20 mins)...");
            GeneratePeopleList(false);
            AddLog("Done. Count: " + UMBRACO_PERSON_LOOKUP.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Umbraco Subsites...");
            UMBRACO_SUBSITES_LOOKUP = GetSubSitesFromUmbracoAll();
            AddLog("Done. Count: " + UMBRACO_SUBSITES_LOOKUP.Count, LogFormat.Success);
            AddLog("");

            //AddLog("Getting Mode Code Folders From Umbraco...");
            //GenerateModeCodeFolderList(false);
            //AddLog("Done. Count: " + MODE_CODE_FOLDER_LIST.Count);
            //AddLog("");

            AddLog("Getting New Mode Codes...");
            MODE_CODE_NEW_LIST = GetNewModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Umbraco Docs...");
            UMBRACO_DOC_LOOKUP = GetUmbracoDocsAll();
            AddLog("Done. Count: " + UMBRACO_DOC_LOOKUP.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Umbraco Old Urls...");
            UMBRACO_OLD_URL_LOOKUP = GetUmbracoOldUrlAll();
            AddLog("Done. Count: " + UMBRACO_OLD_URL_LOOKUP.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Valid Sites...");
            VALID_SITES = GetValidSubSitesAll();
            AddLog("Done. Count: " + VALID_SITES.Count, LogFormat.Success);
            AddLog("");

            AddLog("Getting Valid Docs...");
            VALID_DOCS = GetValidDocsAll();
            AddLog("Done. Count: " + VALID_DOCS.Count, LogFormat.Success);
            AddLog("");
            AddLog("");


            if (true == runImportNav)
            {
               ImportNavs();
            }
            if (true == runLinkNav)
            {
               LinkNavs();
            }
            if (true == runFixNav)
            {
               FixNavs();
            }

         }

         TIME_ENDED = DateTime.Now;

         TimeSpan timeLength = TIME_ENDED.Subtract(TIME_STARTED);

         AddLog("/// Time to complete: " + timeLength.ToString(@"hh") + " hours : " + timeLength.ToString(@"mm") + " minutes : " + timeLength.ToString(@"ss") + " seconds ///", LogFormat.Info);
         AddLog("");

         using (FileStream fs = File.Create("NAVIGATION_LOG_FILE.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }



      static void ImportNavs()
      {
         // DELETE CACHE FILE
         if (File.Exists("NAVIGATION_imported-nav.txt"))
         {
            File.Delete("NAVIGATION_imported-nav.txt");
         }

         List<NavSystem> navSysModeCodeList = NavSystems.GetNavModeCodeList();

         // Import the Navs
         if (navSysModeCodeList != null)
         {
            navSysModeCodeList = navSysModeCodeList.Where(p => p.OriginSiteType != "ad_hoc").ToList();
            navSysModeCodeList = navSysModeCodeList.OrderBy(p => p.OriginSiteType).ThenBy(x => x.OriginSiteId).ToList();

            int navSysCount = navSysModeCodeList.Count;
            int navSysInc = 1;

            foreach (NavSystem navSysModeCodeItem in navSysModeCodeList)
            {
               if (navSysInc >= StartRecord)
               {
                  AddLog("");
                  AddLog("Record " + navSysInc + " of " + navSysCount);
                  AddLog("Site Type: " + navSysModeCodeItem.OriginSiteType + " // Origin Site ID: " + navSysModeCodeItem.OriginSiteId);

                  ////////////////////////////////////////////////////////////////
                  //== PROGRAM
                  if (navSysModeCodeItem.OriginSiteType.ToLower() == "program")
                  {
                     List<NavSystem> filteredNavSysCode = NavSystems.GetNavSysListByPlace(navSysModeCodeItem.OriginSiteId, navSysModeCodeItem.OriginSiteType);

                     ProgramLookup programCode = PROGRAM_LIST.Where(p => p.ProgramCode == navSysModeCodeItem.OriginSiteId).FirstOrDefault();

                     if (programCode != null)
                     {
                        AddLog(" - NP Code Found: " + programCode.ProgramCode + " umbId: " + programCode.UmbracoId);

                        CreateLeftNav(programCode.NavUmbracoId, navSysModeCodeItem.OriginSiteType + " // " + navSysModeCodeItem.OriginSiteId, filteredNavSysCode);
                     }
                     else
                     {
                        AddLog("!! NP Code NOT Found (Most Likely Old NP Code): " + navSysModeCodeItem.OriginSiteId, LogFormat.Warning);
                     }
                  }


                  ////////////////////////////////////////////////////////////////
                  //== SUBSITE
                  if (navSysModeCodeItem.OriginSiteType.ToLower() == "subsite")
                  {
                     List<NavSystem> filteredNavSysCode = NavSystems.GetNavSysListByPlace(navSysModeCodeItem.OriginSiteId, navSysModeCodeItem.OriginSiteType);

                     SubsiteLookup subsiteLookup = UMBRACO_SUBSITES_LOOKUP.Where(p => p.Subsite == navSysModeCodeItem.OriginSiteId).FirstOrDefault();

                     if (subsiteLookup != null)
                     {
                        AddLog(" - Subsite Found: " + subsiteLookup.Subsite + " umbId: " + subsiteLookup.UmbracoId);

                        CreateLeftNav(subsiteLookup.NavUmbracoId, navSysModeCodeItem.OriginSiteType + " // " + navSysModeCodeItem.OriginSiteId, filteredNavSysCode);
                     }
                     else
                     {
                        AddLog("!! NP Code NOT Found (Most Likely Old NP Code): " + navSysModeCodeItem.OriginSiteId, LogFormat.Warning);
                     }
                  }


                  ////////////////////////////////////////////////////////////////
                  //== PLACE
                  else if (navSysModeCodeItem.OriginSiteType.ToLower() == "place")
                  {
                     string siteId = navSysModeCodeItem.OriginSiteId;

                     List<NavSystem> filteredNavSysModeCode = NavSystems.GetNavSysListByPlace(siteId, navSysModeCodeItem.OriginSiteType);

                     List<NavSystem> filteredNavSysAdHoc = NavSystems.GetNavSysListByAdHoc(siteId);

                     if (filteredNavSysAdHoc == null)
                     {
                        List<ModeCodeNew> modeCodeNewList = MODE_CODE_NEW_LIST.Where(p => p.ModecodeNew == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(siteId)).ToList();

                        if (modeCodeNewList != null && modeCodeNewList.Any())
                        {
                           foreach (ModeCodeNew oldModeCode in modeCodeNewList)
                           {
                              filteredNavSysAdHoc = NavSystems.GetNavSysListByAdHoc(Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(oldModeCode.ModecodeOld));

                              if (filteredNavSysAdHoc != null & filteredNavSysAdHoc.Any())
                              {
                                 filteredNavSysModeCode.AddRange(filteredNavSysAdHoc);
                              }
                           }
                        }
                     }
                     else if (filteredNavSysAdHoc != null && filteredNavSysAdHoc.Any())
                     {
                        filteredNavSysModeCode.AddRange(filteredNavSysAdHoc);
                     }

                     ModeCodeLookup modeCode = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(siteId)).FirstOrDefault();

                     if (modeCode != null)
                     {
                        AddLog(" - Mode Code Found: " + modeCode.ModeCode + " umbId: " + modeCode.UmbracoId);

                        // UPDATE FOR NEWS

                        if (modeCode.ModeCode == "00-00-00-00")
                        {
                           List<NavSystem> filteredNavSysNews = filteredNavSysModeCode.Where(p => p.BBSect == "News").ToList();
                           List<NavSystem> filteredNavSysNonNews = filteredNavSysModeCode.Where(p => p.BBSect != "News").ToList();

                           CreateLeftNav(165575, "News", filteredNavSysNews);

                           CreateLeftNav(modeCode.NavUmbracoId, "ARS Home", filteredNavSysNonNews);
                        }
                        else
                        {
                           CreateLeftNav(modeCode.NavUmbracoId, modeCode.ModeCode, filteredNavSysModeCode);
                        }

                     }
                     else
                     {
                        AddLog("!! Mode Code NOT Found (Most Likely Old Mode Code): " + navSysModeCodeItem.OriginSiteId, LogFormat.Warning);
                     }
                  }

                  ////////////////////////////////////////////////////////////////
                  //== PERSON
                  else if (navSysModeCodeItem.OriginSiteType.ToLower() == "person")
                  {
                     string personId = navSysModeCodeItem.OriginSiteId;
                     int personIdInt = 0;
                     int umbracoPersonNodeId = 0;

                     if (true == int.TryParse(personId, out personIdInt))
                     {
                        PersonLookup personUmbracoNode = UMBRACO_PERSON_LOOKUP.Where(p => p.PersonId == personIdInt).FirstOrDefault();

                        if (personUmbracoNode != null)
                        {
                           AddLog(" - Person Site Found: " + personUmbracoNode.PersonId + " umbId: " + personUmbracoNode.UmbracoId);

                           umbracoPersonNodeId = personUmbracoNode.UmbracoId;

                           UmbracoDocLookup personParentModeNode = GetModeCodeByUmbracoPersonId(personId);

                           if (personParentModeNode != null)
                           {
                              List<NavSystem> filteredNavSysModeCode = NavSystems.GetNavSysListByPlace(Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(personId.ToString()), "person");

                              if (filteredNavSysModeCode != null)
                              {
                                 if (filteredNavSysModeCode != null)
                                 {
                                    CreateLeftNav(personUmbracoNode.NavUmbracoId, "Person ID: " + navSysModeCodeItem.OriginSiteId, filteredNavSysModeCode);
                                 }
                              }
                           }
                        }
                        else
                        {
                           AddLog("!! Person Site NOT Found: " + navSysModeCodeItem.OriginSiteId, LogFormat.Warning);
                        }
                     }
                     else
                     {
                        AddLog("!! Invalid Person ID: " + navSysModeCodeItem.OriginSiteId, LogFormat.Error);
                     }
                  }
               }
               else
               {
                  AddLog("...Bypassing...");
               }

               navSysInc++;
            }
         }

         using (FileStream fs = File.Create("NAVIGATION_LOG_FILE_IMPORT_NAVS.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }


      static void LinkNavs()
      {
         int recordNum = 1;
         int recordTotal = 0;

         AddLog("= Link Nav to Umbraco Nodes =", LogFormat.Header);

         IMPORTED_NAV = GetImportedNavCache();

         if (UMBRACO_OLD_URL_LOOKUP != null)
         {
            recordTotal = UMBRACO_OLD_URL_LOOKUP.Count;

            foreach (UmbracoDocLookup oldNode in UMBRACO_OLD_URL_LOOKUP)
            {
               AddLog("");
               AddLog("Record " + recordNum + " / " + recordTotal);
               AddLog("URL: " + oldNode.OldUrl, LogFormat.Gray);
               AddLog("Umbraco ID: " + oldNode.UmbracoId);

               if (oldNode != null)
               {
                  NavByPage navByPage = null;

                  if (oldNode.OldUrl.Contains("/PandP/locations/cityPeopleList.cfm?modeCode="))
                  {
                     navByPage = new NavByPage() { NavLeft = 0, NavRight = 0, NavMain = UpdateUmbracoPageLinkNav("", "pandp") };
                  }
                  else if (oldNode.OldUrl.Contains("/is/pr/"))
                  {
                     navByPage = new NavByPage() { NavLeft = 0, NavRight = 23, NavMain = "" };
                  }
                  else if (oldNode.OldUrl.Contains("/research/programs/programs.htm?np_code=") && oldNode.OldUrl.Contains("&docid="))
                  {
                     navByPage = null;
                  }
                  else if (oldNode.OldUrl.Contains("/main/site_main.htm?modecode="))
                  {
                     navByPage = null;
                  }
                  else if (oldNode.OldUrl.Contains("/services/software/software.htm"))
                  {
                     navByPage = null;
                  }


                  else
                  {
                     AddLog(" - Getting production page info...");
                     navByPage = GetNavsByProduction(oldNode.OldUrl);
                  }

                  if (navByPage != null)
                  {
                     List<ImportedNav> importedNavList = new List<ImportedNav>();

                     ImportedNav foundNav = null;

                     if (navByPage.NavLeft > 0)
                     {
                        AddLog(" - Nav Left Found: " + navByPage.NavLeft);

                        foundNav = IMPORTED_NAV.Where(p => p.NavSysId == navByPage.NavLeft).FirstOrDefault();

                        if (foundNav != null)
                        {
                           importedNavList.Add(foundNav);
                        }
                     }
                     if (navByPage.NavRight > 0)
                     {
                        AddLog(" - Nav Right Found: " + navByPage.NavRight);

                        foundNav = IMPORTED_NAV.Where(p => p.NavSysId == navByPage.NavRight).FirstOrDefault();

                        if (foundNav != null)
                        {
                           importedNavList.Add(foundNav);
                        }
                     }


                     if ((importedNavList != null && importedNavList.Any()) || false == string.IsNullOrEmpty(navByPage.NavMain))
                     {
                        string jsonNav = "";

                        if (importedNavList != null && importedNavList.Any())
                        {
                           jsonNav = LinkLeftNavItemsList(importedNavList);
                        }

                        AddLog(" - Updating...");
                        ApiResponse apiResponse = UpdateUmbracoPageNav(oldNode.UmbracoId, jsonNav, navByPage.NavMain);

                        if (apiResponse != null && apiResponse.ContentList != null && apiResponse.ContentList.Count == 1)
                        {
                           if (apiResponse.ContentList[0].Success)
                           {
                              AddLog(" - Saved and Published: (" + apiResponse.ContentList[0].Id + ") " + apiResponse.ContentList[0].Name, LogFormat.Success);
                           }
                           else
                           {
                              AddLog(" !! Couldn't save nav. " + apiResponse.ContentList[0].Message, LogFormat.Error);
                           }
                        }
                        else if (apiResponse != null)
                        {
                           AddLog(" !! Couldn't save nav" + apiResponse.Message, LogFormat.Error);
                        }
                        else
                        {
                           AddLog(" !! Couldn't save nav", LogFormat.Error);
                        }
                     }
                     else
                     {
                        AddLog(" - Couldn't find Nav (Left: " + navByPage.NavLeft + " | Right: " + navByPage.NavRight + ") in Umbraco", LogFormat.Warning);
                     }
                  }
                  else
                  {
                     AddLog(" - No Nav Found", LogFormat.Okay);
                  }
               }

               recordNum++;
            }
         }


         using (FileStream fs = File.Create("NAVIGATION_LOG_FILE_LINK_NAVS.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }


      static void FixNavs()
      {
         List<UmbracoPropertyData> brokenNavList = GetBrokenNavLinks();

         var jsonSettings = new JsonSerializerSettings();
         jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

         int recordNum = 1;
         int recordTotal = 0;

         if (brokenNavList != null)
         {
            recordTotal = brokenNavList.Count;

            foreach (UmbracoPropertyData data in brokenNavList)
            {
               bool updatedNav = false;

               int id = data.contentNodeId;

               string leftNavJson = data.dataNtext;

               AddLog("Record: " + recordNum + " / " + recordTotal);
               AddLog("Found Nav: Umbraco ID: " + data.contentNodeId, LogFormat.Info);

               ApiArchetype navArchetypeItem = JsonConvert.DeserializeObject<ApiArchetype>(leftNavJson);

               if (navArchetypeItem != null && navArchetypeItem.Fieldsets != null && navArchetypeItem.Fieldsets.Any())
               {
                  if (navArchetypeItem.Fieldsets[0].Properties.Any())
                  {
                     AddLog(" - Found Nav Picker Item.");

                     foreach (Property prop in navArchetypeItem.Fieldsets[0].Properties)
                     {
                        Dictionary dictionary = JsonConvert.DeserializeObject<Dictionary>(prop.Value);

                        if (dictionary != null)
                        {
                           if (dictionary.Text == null || false == dictionary.Text.Contains("//"))
                           {
                              int navNodeId = Convert.ToInt32(dictionary.Value);

                              if (navNodeId > 0)
                              {
                                 ApiResponse apiNode = GetUmbracoNode(navNodeId);

                                 AddLog(" - Getting Nav Folder...");

                                 ModeCodeLookup navLookup = MODE_CODE_LIST.Where(p => p.NavUmbracoId == apiNode.ContentList[0].ParentId).FirstOrDefault();

                                 if (navLookup != null)
                                 {
                                    ApiResponse apiNodeParent = GetUmbracoNode(navLookup.UmbracoId);

                                    AddLog(" - Getting Parent Node...");

                                    if (apiNodeParent != null)
                                    {
                                       string newNavText = apiNode.ContentList[0].Name + " // (" + apiNodeParent.ContentList[0].Name + ")";

                                       dictionary.Text = newNavText;

                                       prop.Value = JsonConvert.SerializeObject(dictionary);

                                       updatedNav = true;
                                       AddLog(" - New nav text: " + newNavText);
                                    }
                                 }
                              }
                           }
                           else
                           {
                              AddLog(" - Navigation already fixed: " + dictionary.Text, LogFormat.Okay);
                           }
                        }
                     }

                     if (true == updatedNav)
                     {
                        AddLog(" - Updating navigiation via API...");

                        string jsonUpdate = JsonConvert.SerializeObject(navArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                        ApiResponse apiResponse = FixUmbracoPageNav(id, jsonUpdate);

                        if (apiResponse != null && apiResponse.ContentList != null && apiResponse.ContentList.Count == 1)
                        {
                           if (apiResponse.ContentList[0].Success)
                           {
                              AddLog(" - Saved and Published: (" + apiResponse.ContentList[0].Id + ") " + apiResponse.ContentList[0].Name, LogFormat.Success);
                           }
                           else
                           {
                              AddLog(" !! Couldn't save nav. " + apiResponse.ContentList[0].Message, LogFormat.Error);
                           }
                        }
                        else if (apiResponse != null)
                        {
                           AddLog(" !! Couldn't save nav" + apiResponse.Message, LogFormat.Error);
                        }
                        else
                        {
                           AddLog(" !! Couldn't save nav", LogFormat.Error);
                        }
                     }
                     else
                     {
                        AddLog(" - No update required.", LogFormat.Okay);
                     }
                  }
               }

               AddLog("");
               recordNum++;
            }
         }



         //FixUmbracoPageNav(id, leftNav);
      }


      static void GenerateModeCodeList(bool forceCacheUpdate)
      {
         MODE_CODE_LIST = GetModeCodeLookupCache();

         if (true == forceCacheUpdate || MODE_CODE_LIST == null || MODE_CODE_LIST.Count <= 0)
         {
            MODE_CODE_LIST = CreateModeCodeLookupCache();
         }
      }

      static List<ModeCodeLookup> GetModeCodeLookupCache()
      {
         string filename = "NAVIGATION_mode-code-cache.txt";
         List<ModeCodeLookup> modeCodeList = new List<ModeCodeLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  modeCodeList.Add(new ModeCodeLookup() { ModeCode = lineArray[0], UmbracoId = Convert.ToInt32(lineArray[1]), NavUmbracoId = Convert.ToInt32(lineArray[2]), Url = lineArray[3] });
               }
            }
         }

         return modeCodeList;
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
               sb.AppendLine(modeCodeItem.ModeCode + "|" + modeCodeItem.UmbracoId + "|" + modeCodeItem.NavUmbracoId + "|" + modeCodeItem.Url);
            }

            using (FileStream fs = File.Create("NAVIGATION_mode-code-cache.txt"))
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

         ApiResponse responseBack = ApiCalls.PostData(request, "GetAllModeCodeNodesWithSubNodes");

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
                        string oldUrl = "";

                        ApiProperty oldUrlProp = node.Properties.Where(p => p.Key == "oldUrl").FirstOrDefault();

                        if (oldUrlProp != null)
                        {
                           oldUrl = oldUrlProp.Value.ToString();
                        }

                        int umbracoNavId = 0;

                        if (node.ChildContentList != null && node.ChildContentList.Any())
                        {
                           ApiContent nodeNav = node.ChildContentList.Where(p => p.DocType == "SiteNavFolder").FirstOrDefault();

                           if (nodeNav != null)
                           {
                              umbracoNavId = nodeNav.Id;
                           }
                        }

                        modeCodeList.Add(new ModeCodeLookup { ModeCode = modeCode.Value.ToString(), UmbracoId = node.Id, NavUmbracoId = umbracoNavId, Url = node.Url, OldUrl = oldUrl, Name = node.Name });

                        AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
                     }
                  }
               }
            }
         }

         modeCodeList.Add(new ModeCodeLookup { ModeCode = "02-00-00-00", UmbracoId = 31703, NavUmbracoId = 165480, Url = "/docs/", OldUrl = "" });
         modeCodeList.Add(new ModeCodeLookup { ModeCode = "01-09-00-00", UmbracoId = 2220, NavUmbracoId = 165712, Url = "/office-of-technology-transfer/", OldUrl = "/Business/business.htm,/ott" });


         return modeCodeList;
      }



      static void GenerateProgramList(bool forceCacheUpdate)
      {
         PROGRAM_LIST = GetProgramLookupCache();

         if (true == forceCacheUpdate || PROGRAM_LIST == null || PROGRAM_LIST.Count <= 0)
         {
            PROGRAM_LIST = CreateProgramLookupCache();
         }
      }

      static List<ProgramLookup> GetProgramLookupCache()
      {
         string filename = "NAVIGATION_program-cache.txt";
         List<ProgramLookup> programList = new List<ProgramLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  programList.Add(new ProgramLookup() { ProgramCode = lineArray[0], UmbracoId = Convert.ToInt32(lineArray[1]), NavUmbracoId = Convert.ToInt32(lineArray[2]), Url = lineArray[3] });
               }
            }
         }

         return programList;
      }

      static List<ProgramLookup> CreateProgramLookupCache()
      {
         List<ProgramLookup> programList = new List<ProgramLookup>();

         programList = GetProgramsAll();

         StringBuilder sb = new StringBuilder();

         if (programList != null)
         {
            foreach (ProgramLookup programItem in programList)
            {
               sb.AppendLine(programItem.ProgramCode + "|" + programItem.UmbracoId + "|" + programItem.NavUmbracoId + "|" + programItem.Url);
            }

            using (FileStream fs = File.Create("NAVIGATION_program-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return programList;
      }

      static List<ProgramLookup> GetProgramsAll()
      {
         List<ProgramLookup> modeCodeList = new List<ProgramLookup>();
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;

         ApiResponse responseBack = ApiCalls.PostData(request, "GetAllNationalProgramNodes");

         if (responseBack != null && responseBack.Success)
         {
            if (responseBack.ContentList != null && responseBack.ContentList.Any())
            {
               foreach (ApiContent node in responseBack.ContentList)
               {
                  if (node != null)
                  {
                     ApiProperty npCode = node.Properties.Where(p => p.Key == "npCode").FirstOrDefault();

                     if (npCode != null)
                     {
                        string oldUrl = "";

                        ApiProperty oldUrlProp = node.Properties.Where(p => p.Key == "oldUrl").FirstOrDefault();

                        if (oldUrlProp != null)
                        {
                           oldUrl = oldUrlProp.Value.ToString();
                        }

                        int umbracoNavId = 0;

                        if (node.ChildContentList != null && node.ChildContentList.Any())
                        {
                           ApiContent nodeNav = node.ChildContentList.Where(p => p.DocType == "SiteNavFolder").FirstOrDefault();

                           if (nodeNav != null)
                           {
                              umbracoNavId = nodeNav.Id;
                           }
                        }

                        modeCodeList.Add(new ProgramLookup { ProgramCode = npCode.Value.ToString(), UmbracoId = node.Id, NavUmbracoId = umbracoNavId, Url = node.Url, OldUrl = oldUrl, Name = node.Name });

                        AddLog(" - Adding NP Code (" + npCode.Value + "):" + node.Name);
                     }
                  }
               }
            }
         }

         return modeCodeList;
      }



      static void GeneratePeopleList(bool forceCacheUpdate)
      {
         UMBRACO_PERSON_LOOKUP = GetPersonLookupCache();

         if (true == forceCacheUpdate || UMBRACO_PERSON_LOOKUP == null || UMBRACO_PERSON_LOOKUP.Count <= 0)
         {
            UMBRACO_PERSON_LOOKUP = CreatePersonLookupCache();
         }
      }

      static List<PersonLookup> GetPersonLookupCache()
      {
         string filename = "NAVIGATION_person-cache.txt";
         List<PersonLookup> personList = new List<PersonLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  personList.Add(new PersonLookup() { PersonId = Convert.ToInt32(lineArray[0]), UmbracoId = Convert.ToInt32(lineArray[1]), NavUmbracoId = Convert.ToInt32(lineArray[2]), Name = lineArray[3] });
               }
            }
         }

         return personList;
      }

      static List<PersonLookup> CreatePersonLookupCache()
      {
         List<PersonLookup> personList = new List<PersonLookup>();

         personList = GetPeopleAll();

         StringBuilder sb = new StringBuilder();

         if (personList != null)
         {
            foreach (PersonLookup programItem in personList)
            {
               sb.AppendLine(programItem.PersonId + "|" + programItem.UmbracoId + "|" + programItem.NavUmbracoId +"|" + programItem.Name);
            }

            using (FileStream fs = File.Create("NAVIGATION_person-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return personList;
      }

      static List<PersonLookup> GetPeopleAll()
      {
         List<PersonLookup> personList = new List<PersonLookup>();
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
                     ApiProperty personId = node.Properties.Where(p => p.Key == "personLink").FirstOrDefault();

                     if (personId != null)
                     {
                        string oldUrl = "";

                        ApiProperty oldUrlProp = node.Properties.Where(p => p.Key == "oldUrl").FirstOrDefault();

                        if (oldUrlProp != null)
                        {
                           oldUrl = oldUrlProp.Value.ToString();
                        }

                        int umbracoNavId = 0;

                        if (node.ChildContentList != null && node.ChildContentList.Any())
                        {
                           ApiContent nodeNav = node.ChildContentList.Where(p => p.DocType == "SiteNavFolder").FirstOrDefault();

                           if (nodeNav != null)
                           {
                              umbracoNavId = nodeNav.Id;
                           }
                        }

                        personList.Add(new PersonLookup { PersonId = Convert.ToInt32(personId.Value), UmbracoId = node.Id, NavUmbracoId = umbracoNavId, Name = node.Name });

                        AddLog(" - Adding NP Code (" + personId.Value + "):" + node.Name);
                     }
                  }
               }
            }
         }

         return personList;
      }




      static void GenerateModeCodeFolderList(bool forceCacheUpdate)
      {
         MODE_CODE_FOLDER_LIST = GetModeCodeFolderLookupCache();

         if (true == forceCacheUpdate || MODE_CODE_FOLDER_LIST == null || MODE_CODE_FOLDER_LIST.Count <= 0)
         {
            MODE_CODE_FOLDER_LIST = CreateModeCodeFolderLookupCache();
         }
      }

      static List<ModeCodeFolderLookup> GetModeCodeFolderLookupCache()
      {
         string filename = "NAVIGATION_mode-code-folder-cache.txt";
         List<ModeCodeFolderLookup> modeCodeList = new List<ModeCodeFolderLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  modeCodeList.Add(new ModeCodeFolderLookup() { ModeCode = lineArray[0], FolderName = lineArray[1], UmbracoId = Convert.ToInt32(lineArray[2]) });
               }
            }
         }

         return modeCodeList;
      }

      static List<ModeCodeFolderLookup> CreateModeCodeFolderLookupCache()
      {
         List<ModeCodeFolderLookup> modeCodeFolderList = new List<ModeCodeFolderLookup>();

         modeCodeFolderList = GetModeCodeFoldersAll();

         StringBuilder sb = new StringBuilder();

         if (modeCodeFolderList != null)
         {
            foreach (ModeCodeFolderLookup modeCodeItem in modeCodeFolderList)
            {
               sb.AppendLine(modeCodeItem.ModeCode + "|" + modeCodeItem.FolderName + "|" + modeCodeItem.UmbracoId);
            }

            using (FileStream fs = File.Create("NAVIGATION_mode-code-folder-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return modeCodeFolderList;
      }

      static List<ModeCodeFolderLookup> GetModeCodeFoldersAll()
      {
         List<ModeCodeFolderLookup> modeCodeFolderList = new List<ModeCodeFolderLookup>();
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;

         foreach (ModeCodeLookup modeCode in MODE_CODE_LIST)
         {
            ApiResponse responseBack = GetCalls.GetNodeByUmbracoId(modeCode.UmbracoId);

            if (responseBack != null && responseBack.Success)
            {
               if (responseBack.ContentList != null && responseBack.ContentList.Any() && responseBack.ContentList[0].ChildContentList != null && responseBack.ContentList[0].ChildContentList.Any())
               {
                  foreach (ApiContent node in responseBack.ContentList[0].ChildContentList)
                  {
                     if (node != null)
                     {
                        if (node.DocType != "City" && node.DocType != "Area" && node.DocType != "ResearchUnit")
                        {
                           modeCodeFolderList.Add(new ModeCodeFolderLookup { ModeCode = modeCode.ModeCode, FolderName = node.Name, UmbracoId = node.Id });
                           AddLog(" - Adding: " + node.Name + " (" + node.Id + ")");
                        }

                     }
                  }
               }
            }
         }


         return modeCodeFolderList;
      }


      static List<SubsiteLookup> GetSubSitesFromUmbracoAll()
      {
         List<SubsiteLookup> lookupList = new List<SubsiteLookup>();

         lookupList.Add(new SubsiteLookup() { Subsite = "sciQualRev", UmbracoId = 2133, NavUmbracoId = 165708 });
         lookupList.Add(new SubsiteLookup() { Subsite = "HQsubsite", UmbracoId = 130737, NavUmbracoId = 165709 });
         lookupList.Add(new SubsiteLookup() { Subsite = "ARSLegisAffrs", UmbracoId = 130737, NavUmbracoId = 165710 });
         lookupList.Add(new SubsiteLookup() { Subsite = "odeo", UmbracoId = 130739, NavUmbracoId = 165711 });
         lookupList.Add(new SubsiteLookup() { Subsite = "irp", UmbracoId = 130729, NavUmbracoId = 165713 });
         lookupList.Add(new SubsiteLookup() { Subsite = "CEAP", UmbracoId = 130740, NavUmbracoId = 165715 });
         lookupList.Add(new SubsiteLookup() { Subsite = "01090000", UmbracoId = 2220, NavUmbracoId = 165712 });


         return lookupList;
      }


      static void GenerateNavImportList(bool forceCacheUpdate)
      {
         IMPORTED_NAV = GetNavImportLookupCache();

         if (true == forceCacheUpdate || IMPORTED_NAV == null || IMPORTED_NAV.Count <= 0)
         {
            IMPORTED_NAV = CreateNavImportLookupCache();
         }
      }

      static List<ImportedNav> GetNavImportLookupCache()
      {
         string filename = "NAVIGATION_nav-import-cache.txt";
         List<ImportedNav> navImportList = new List<ImportedNav>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('|');

                  navImportList.Add(new ImportedNav() { NavSysId = Convert.ToInt32(lineArray[0]), UmbracoNodeId = Convert.ToInt32(lineArray[1]), NavTitle = lineArray[2], Section = lineArray[3], Label = lineArray[4] });
               }
            }
         }

         return navImportList;
      }


      static List<ImportedNav> CreateNavImportLookupCache()
      {
         List<ImportedNav> navImportList = new List<ImportedNav>();

         navImportList = GetImportedNavAll();

         StringBuilder sb = new StringBuilder();

         if (navImportList != null)
         {
            foreach (ImportedNav navImportItem in navImportList)
            {
               sb.AppendLine(navImportItem.NavSysId + "|" + navImportItem.UmbracoNodeId + "|" + navImportItem.NavTitle + "|" + navImportItem.Section + "|" + navImportItem.Label);
            }

            using (FileStream fs = File.Create("NAVIGATION_nav-import-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return navImportList;
      }

      static List<ImportedNav> GetImportedNavAll()
      {
         List<ImportedNav> importedNavList = new List<ImportedNav>();
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;



         ApiResponse responseBack = ApiCalls.PostData(request, "GetAllNavigationNodes");

         if (responseBack != null && responseBack.Success)
         {
            if (responseBack.ContentList != null && responseBack.ContentList.Any())
            {
               foreach (ApiContent node in responseBack.ContentList)
               {
                  if (node != null)
                  {
                     ApiProperty property = node.Properties.Where(p => p.Key == "navSysID").FirstOrDefault();

                     if (property != null)
                     {
                        string navSysId = property.Value.ToString();

                        importedNavList.Add(new ImportedNav { UmbracoNodeId = node.Id, NavSysId = Convert.ToInt32(navSysId) });

                        AddLog(" - Adding Nav (" + node.Id + "):" + node.Name);
                     }
                  }
               }
            }
         }

         return importedNavList;
      }



      static List<ModeCodeNew> GetNewModeCodesAll()
      {
         List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT * FROM NewModecodes";

         modeCodeNewList = db.Query<ModeCodeNew>(sql).ToList();

         return modeCodeNewList;
      }


      static List<SubSite> GetValidSubSitesAll()
      {
         List<SubSite> validSubSitesList = new List<SubSite>();

         validSubSitesList = SubSites.GetValidSiteList();

         return validSubSitesList;
      }


      static List<Document> GetValidDocsAll()
      {
         List<Document> validDocsList = new List<Document>();

         validDocsList = Documents.GetValidDocList();

         return validDocsList;
      }


      static List<UmbracoDocLookup> GetUmbracoDocsAll()
      {
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'oldId')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar <> '' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         List<UmbracoDocLookup> docList = db.Query<UmbracoDocLookup>(sql).ToList();

         return docList;
      }


      static List<UmbracoDocLookup> GetUmbracoOldUrlAll()
      {
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'oldUrl')
                            AND NOT dataNtext IS NULL AND cast(dataNtext as nvarchar(max)) <> N'' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         List<UmbracoDocLookup> docList = db.Query<UmbracoDocLookup>(sql).ToList();

         return docList;
      }


      static List<UmbracoDocLookup> GetUmbracoPeopleAll()
      {
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'personLink')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar <> '' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         List<UmbracoDocLookup> docList = db.Query<UmbracoDocLookup>(sql).ToList();

         return docList;
      }



      static void PublishChildren(int parentNodeId)
      {
         AddLog(" --- Publishing pages...");

         ApiRequest requestPublish3 = new ApiRequest();
         ApiContent contentPublish3 = new ApiContent();

         requestPublish3.ApiKey = API_KEY;

         contentPublish3.Id = parentNodeId;

         requestPublish3.ContentList = new List<ApiContent>();
         requestPublish3.ContentList.Add(contentPublish3);

         ApiResponse responseBackPublish3 = ApiCalls.PostData(requestPublish3, "PublishWithChildren", 120000);

         if (responseBackPublish3 != null)
         {
            AddLog(" --- Success: " + responseBackPublish3.Success);
            AddLog(" --- Message: " + responseBackPublish3.Message);
         }
      }


      static UmbracoDocLookup GetModeCodeByUmbracoPersonId(string personId)
      {
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'modeCode')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar <> '' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)
							AND contentNodeId IN (
                        SELECT Id FROM umbracoNode WHERE id IN (SELECT parentID FROM umbracoNode WHERE id IN (SELECT parentID FROM umbracoNode WHERE id IN (
						
						SELECT contentNodeId FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'personLink')
                            AND dataNvarchar = @personId AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)
						))))";

         UmbracoDocLookup docList = db.Query<UmbracoDocLookup>(sql, new { personId = personId }).FirstOrDefault();

         return docList;
      }


      static void CreateLeftNav(int navUmbracoId, string location, List<NavSystem> navSysList)
      {
         bool publishPages = false;

         if (navSysList != null && navSysList.Any())
         {
            foreach (NavSystem navSys in navSysList)
            {
               AddLog(" - Nav Sys ID: " + navSys.NavSysId);

               ApiContent content = new ApiContent();
               List<Navigation> navItemsList = Navigations.GetNavigationList(navSys.NavSysId);

               if (navItemsList != null && navItemsList.Any())
               {
                  string title = "";
                  string navItemsJson = CreateLeftNavItemsList(navItemsList);

                  if (false == string.IsNullOrWhiteSpace(navSys.NavSysLabel))
                  {
                     title += navSys.NavSysLabel;
                  }
                  else
                  {
                     title += "Navigation";
                  }

                  if (false == string.IsNullOrWhiteSpace(navSys.BBSect))
                  {
                     title += " - " + navSys.BBSect;
                  }

                  //if (false == string.IsNullOrWhiteSpace(location))
                  //{
                  //   title += " (" + location + ")";
                  //}
                  if (false == string.IsNullOrWhiteSpace(navSys.NavPageLoc))
                  {
                     if (navSys.NavPageLoc.ToLower() == "right")
                     {
                        title += " [R]";
                     }
                     else if (navSys.NavPageLoc.ToLower() == "left")
                     {
                        title += " [L]";
                     }
                     else if (navSys.NavPageLoc.ToLower() == "related")
                     {
                        title += " [REL]";
                     }
                  }

                  if (navSys.ParentNodeTitle != null)
                  {
                     title += " // (" + navSys.ParentNodeTitle + ")";
                  }


                  content.Id = 0;
                  content.Name = title;
                  content.ParentId = navUmbracoId;
                  content.DocType = "LeftNavigationSet";
                  content.Template = "";

                  List<ApiProperty> properties = new List<ApiProperty>();

                  properties.Add(new ApiProperty("navigationItems", navItemsJson)); // Nav List JSON
                  properties.Add(new ApiProperty("navSysID", navSys.NavSysId)); // Old Nav Sys ID

                  content.Properties = properties;

                  content.Save = 1;

                  ApiRequest request = new ApiRequest();

                  request.ContentList = new List<ApiContent>();
                  request.ContentList.Add(content);
                  request.ApiKey = API_KEY;

                  AddLog(" - Saving...");

                  ApiResponse responseBack = ApiCalls.PostData(request, "Post", 60000);

                  if (responseBack != null && responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                  {
                     if (true == responseBack.ContentList[0].Success)
                     {
                        AddLog(" - Nav Saved: (Umbraco ID: " + responseBack.ContentList[0].Id + ")" + responseBack.ContentList[0].Name, LogFormat.Success);

                        publishPages = true;
                     }
                     else
                     {
                        AddLog("!! " + responseBack.ContentList[0].Message, LogFormat.Error);
                     }
                  }
                  else
                  {
                     AddLog("!! ERROR: Could not save nav: " + title, LogFormat.Error);
                     if (responseBack != null && false == responseBack.Success)
                     {
                        AddLog("!! " + responseBack.Message, LogFormat.Error);
                     }
                     else
                     {
                        AddLog("!! Api returned null", LogFormat.Error);
                     }

                  }
               }
               else
               {
                  AddLog(" - !! Empty Navigation list. Bypassing...", LogFormat.Warning);
               }
            }
         }
         else
         {
            AddLog(" - !! No navigations found.");
         }

         if (true == publishPages)
         {
            PublishChildren(navUmbracoId);
         }

         AddLog("");
      }


      static void UpdateImportedNav(ImportedNav importedNav)
      {
         IMPORTED_NAV.Add(importedNav);

         using (StreamWriter sw = File.AppendText("NAVIGATION_imported-nav.txt"))
         {
            sw.Write(importedNav.NavSysId + "~");
            sw.Write(importedNav.UmbracoNodeId + "~");
            sw.Write(importedNav.UmbracoGuid + "~");
            sw.Write(importedNav.NavTitle + "~");
            sw.Write(importedNav.Section + "~");
            sw.Write(importedNav.Label);
            sw.WriteLine();
         }
      }


      static List<ImportedNav> GetImportedNavCache()
      {
         List<ImportedNav> importedNavList = new List<ImportedNav>();

         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'navSysID')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar <> '' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         List<UmbracoDocLookup> docList = db.Query<UmbracoDocLookup>(sql).ToList();

         if (docList != null)
         {
            foreach (UmbracoDocLookup umbDoc in docList)
            {
               ImportedNav importedNav = new ImportedNav();

               importedNav.UmbracoNodeId = umbDoc.UmbracoId;
               importedNav.NavSysId = Convert.ToInt32(umbDoc.DocId);

               sql = @"SELECT * FROM umbracoNode WHERE Id = " + umbDoc.UmbracoId;

               UmbracoNode umbNode = db.Query<UmbracoNode>(sql).FirstOrDefault();

               if (umbNode != null)
               {
                  importedNav.NavTitle = umbNode.text;
               }
               else
               {
                  importedNav.NavTitle = "Nav";
               }

               importedNavList.Add(importedNav);
            }
         }

         return importedNavList;
      }


      static string CreateLeftNavItemsList(List<Navigation> navItemsList)
      {
         string output = "";

         if (navItemsList != null && navItemsList.Any())
         {
            navItemsList = navItemsList.OrderBy(p => p.RowNum).ToList();

            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

            ApiArchetype navArchetypeItem = new ApiArchetype();
            navArchetypeItem.Fieldsets = new List<Fieldset>();

            foreach (Navigation navItem in navItemsList)
            {
               if (navItem != null && (false == string.IsNullOrWhiteSpace(navItem.NavURL) || false == string.IsNullOrWhiteSpace(navItem.NavLabel)))
               {
                  // LOOP START
                  Fieldset fieldsetTopic = new Fieldset();

                  if (true == string.IsNullOrWhiteSpace(navItem.NavURL) && false == string.IsNullOrWhiteSpace(navItem.NavLabel))
                  {
                     fieldsetTopic.Alias = "topicsHeader";
                     fieldsetTopic.Disabled = false;
                     fieldsetTopic.Id = Guid.NewGuid();
                     fieldsetTopic.Properties = new List<Property>();
                     fieldsetTopic.Properties.Add(new Property("sectionTitle", navItem.NavLabel));
                  }
                  else if (false == string.IsNullOrWhiteSpace(navItem.NavURL) && false == string.IsNullOrWhiteSpace(navItem.NavLabel))
                  {
                     navItem.NavURL = CleanHtml.CleanUpHtml(navItem.NavURL);

                     fieldsetTopic.Alias = "topicsItem";
                     fieldsetTopic.Disabled = false;
                     fieldsetTopic.Id = Guid.NewGuid();
                     fieldsetTopic.Properties = new List<Property>();

                     fieldsetTopic.Properties.Add(new Property("title", navItem.NavLabel));

                     string urlNav = navItem.NavURL.ToLower();

                     if (urlNav.IndexOf("/ARSUserFiles/") >= 0 && (urlNav.EndsWith(".pdf") || urlNav.EndsWith(".doc") || urlNav.EndsWith(".jpg") || urlNav.EndsWith(".xls") ||
                             urlNav.EndsWith(".png") || urlNav.EndsWith(".gif")))
                     {
                        fieldsetTopic.Properties.Add(new Property("linkToFile", navItem.NavURL));
                        fieldsetTopic.Properties.Add(new Property("location", ""));
                     }
                     else
                     {
                        Link navLink = new Link(navItem.NavURL, navItem.NavURL, ""); // set the url path
                        fieldsetTopic.Properties.Add(new Property("location", "[" + JsonConvert.SerializeObject(navLink, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));
                        fieldsetTopic.Properties.Add(new Property("linkToFile", ""));
                     }
                  }

                  if (false == string.IsNullOrWhiteSpace(navItem.NavURL) || false == string.IsNullOrWhiteSpace(navItem.NavLabel))
                  {
                     navArchetypeItem.Fieldsets.Add(fieldsetTopic);
                  }
                  // LOOP END
               }
            }

            if (navArchetypeItem.Fieldsets != null && navArchetypeItem.Fieldsets.Count > 0)
            {
               output = JsonConvert.SerializeObject(navArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
            }
         }

         return output;
      }


      static string LinkLeftNavItemsList(List<ImportedNav> importedNavList)
      {
         string output = "";

         // USED FOR ALL ARCHETYPE DATA TYPES
         var jsonSettings = new JsonSerializerSettings();
         jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

         ApiArchetype navArchetypeItem = new ApiArchetype();

         navArchetypeItem.Fieldsets = new List<Fieldset>();

         if (importedNavList != null)
         {
            foreach (ImportedNav nav in importedNavList)
            {
               string parentNodeName = "";

               ModeCodeLookup modeCodeLookup = MODE_CODE_LIST.Where(p => p.NavUmbracoId == nav.ParentId).FirstOrDefault();

               if (modeCodeLookup != null)
               {
                  parentNodeName = " // (" + modeCodeLookup.Name + ")";
               }
               else
               {
                  ProgramLookup programLookup = PROGRAM_LIST.Where(p => p.NavUmbracoId == nav.ParentId).FirstOrDefault();

                  if (programLookup != null)
                  {
                     parentNodeName = " // (" + programLookup.Name + ")";
                  }
                  else
                  {
                     PersonLookup personLookup = UMBRACO_PERSON_LOOKUP.Where(p => p.NavUmbracoId == nav.ParentId).FirstOrDefault();

                     if (personLookup != null)
                     {
                        parentNodeName = " // (" + personLookup.Name + ")";
                     }
                  }
               }

               // LOOP START
               Fieldset fieldsetNav = new Fieldset();

               fieldsetNav.Alias = "leftNavPicker";
               fieldsetNav.Disabled = false;
               fieldsetNav.Id = Guid.NewGuid();
               fieldsetNav.Properties = new List<Property>();

               Dictionary dictionary = new Dictionary(nav.UmbracoNodeId.ToString(), nav.NavTitle);
               fieldsetNav.Properties.Add(new Property("leftNavList", JsonConvert.SerializeObject(dictionary)));

               navArchetypeItem.Fieldsets.Add(fieldsetNav);
            }
         }

         if (navArchetypeItem.Fieldsets != null && navArchetypeItem.Fieldsets.Count > 0)
         {
            output = JsonConvert.SerializeObject(navArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
         }

         return output;
      }



      static List<UmbracoPropertyData> GetBrokenNavLinks()
      {
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'leftNavPicker')
                            AND dataNtext IS NOT NULL AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         List<UmbracoPropertyData> propertyDataList = db.Query<UmbracoPropertyData>(sql).ToList();

         return propertyDataList;
      }


      static bool UpdateUmbracoPageNavtoDB(int umbracoId, string leftNav)

      {
         bool success = false;

         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'leftNavCreate')
                            AND contentNodeId = @umbracoId AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         UmbracoPropertyData propertyData = db.Query<UmbracoPropertyData>(sql, new { umbracoId = umbracoId }).FirstOrDefault();

         if (propertyData != null)
         {
            propertyData.dataNtext = leftNav;

            db.Update(propertyData);

            ApiContent content = new ApiContent();

            content.Id = umbracoId;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "UpdateXml");

            if (responseBack.Success == true && responseBack.ContentList != null && responseBack.ContentList[0].Success == true)
            {
               success = true;
            }
         }

         return success;
      }


      static ApiResponse UpdateUmbracoPageNav(int id, string leftNav, string mainNav)
      {
         ApiResponse responseBack = null;
         ApiContent content = new ApiContent();

         if (false == string.IsNullOrEmpty(leftNav) || false == string.IsNullOrEmpty(mainNav))
         {
            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            if (false == string.IsNullOrEmpty(leftNav))
            {
               properties.Add(new ApiProperty("leftNavPicker", leftNav)); // 
            }
            if (false == string.IsNullOrEmpty(mainNav))
            {
               properties.Add(new ApiProperty("navigationCategory", mainNav)); // 
            }

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            responseBack = ApiCalls.PostData(request, "Post");
         }

         return responseBack;
      }


      static ApiResponse FixUmbracoPageNav(int id, string leftNav)
      {
         ApiResponse responseBack = null;
         ApiContent content = new ApiContent();

         if (false == string.IsNullOrEmpty(leftNav))
         {
            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("leftNavPicker", leftNav)); // 

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            responseBack = ApiCalls.PostData(request, "Post");
         }

         return responseBack;
      }


      static ApiResponse GetUmbracoNode(int id)
      {
         ApiResponse responseBack = null;
         ApiContent content = new ApiContent();

            content.Id = id;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            responseBack = ApiCalls.PostData(request, "Get");

         return responseBack;
      }


      static string UpdateUmbracoPageLinkNav(string htmlNav, string forceCategory = "")
      {
         string navId = "";
         htmlNav = htmlNav.ToLower();

         // /PandP
         if (forceCategory == "pandp" || (htmlNav.Contains("/pandp/people.htm") && htmlNav.Contains("/pandp/locations.htm") && htmlNav.Contains("/pandp/arsorganchart.htm")))
         {
            navId = "dbc6a2c8-1e09-4f23-9a91-5823a9729626";
         }
         // /Research
         else if (forceCategory == "research" || (htmlNav.Contains("/research/projects_programs.htm?modecode=") && htmlNav.Contains("/research/projectsByLocation.htm?modecode=") && htmlNav.Contains("/research/programs.htm")))
         {
            navId = "f5231859-9053-4e75-835e-2fd07e3575e6";
         }
         // /Services
         else if (forceCategory == "services" || (htmlNav.Contains("/services/services.htm?modecode=") && htmlNav.Contains("/services/TekTran.htm") && htmlNav.Contains("/services/software/software.htm")))
         {
            navId = "6dded150-790b-4ddb-b0dd-15674a066840";
         }
         // /Careers
         else if (forceCategory == "careers" || (htmlNav.Contains("/careers/careers.htm?modecode=") && htmlNav.Contains("/careers/careers.htm") && htmlNav.Contains("https://www.usajobs.gov/jobsearch/search/getresults")))
         {
            navId = "d5f6c79c-a25c-4838-b154-20398f0685e2";
         }
         // /News
         else if (forceCategory == "news" || (htmlNav.Contains("/news/news.htm?modecode=") && htmlNav.Contains("/news/news.htm") && htmlNav.Contains("/is/ar")))
         {
            navId = "ac79b700-ad67-4179-a4f6-db9705ecce31";
         }

         return navId;
      }


      static NavByPage GetNavsByProduction(string url)
      {
         NavByPage navByPage = new NavByPage();

         string urlAddress = "http://www.ars.usda.gov" + url;

         try
         {
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

               string data = readStream.ReadToEnd();

               response.Close();
               readStream.Close();


               if (false == string.IsNullOrEmpty(data))
               {
                  int findLeftSysId = 0;
                  int findRightSysId = 0;

                  int findLeft = 0;
                  int findRight = 0;
                  string htmlNav = "";

                  Match m2 = Regex.Match(data, @"<!--LT:navid:([\d]*)-->", RegexOptions.Singleline);
                  if (m2.Success)
                  {
                     int.TryParse(m2.Groups[1].Value, out findLeft);
                  }

                  Match m3 = Regex.Match(data, @"<!--RT:navid:([\d]*)-->", RegexOptions.Singleline);
                  if (m3.Success)
                  {
                     int.TryParse(m3.Groups[1].Value, out findRight);
                  }

                  Match m4 = Regex.Match(data, @"(<div id=""relatedTopics"">)(.*)(</div>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                  if (m4.Success)
                  {
                     htmlNav = m4.Groups[2].Value;

                     navByPage.NavMain = UpdateUmbracoPageLinkNav(htmlNav);
                  }

                  if (findLeft > 0)
                  {
                     List<Navigation> navList = Navigations.GetNavigationListByNavId(findLeft);

                     if (navList != null && navList.Any())
                     {
                        findLeftSysId = navList[0].NavSysId;
                     }
                  }

                  if (findRight > 0)
                  {
                     List<Navigation> navList = Navigations.GetNavigationListByNavId(findRight);

                     if (navList != null && navList.Any())
                     {
                        findRightSysId = navList[0].NavSysId;
                     }
                  }

                  if (findLeftSysId > 0 || findRightSysId > 0)
                  {
                     navByPage.NavLeft = findLeftSysId;
                     navByPage.NavRight = findRightSysId;
                  }
               }
            }
         }
         catch (Exception ex)
         {
            AddLog("!!! Can't get website page. !!!" + url, LogFormat.Warning);
         }

         return navByPage;
      }


      static bool TestDbConnections()
      {
         bool success = true;

         ///////////////////////////////////////////////////
         AddLog(" - Testing (umbracoDbDSN)...");

         try
         {
            AddLog(" - " + ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString);

            var db = new Database("umbracoDbDSN");

            string sql = @"SELECT TOP 1 * FROM cmsPropertyData";

            List<UmbracoDocLookup> docList = db.Query<UmbracoDocLookup>(sql).ToList();

            AddLog(" - SUCCESS!", LogFormat.Success);
         }
         catch (Exception ex)
         {
            AddLog(" /// ERROR /// Connection Failed!", LogFormat.ErrorBad);
            success = false;
         }
         AddLog("");

         ///////////////////////////////////////////////////
         AddLog(" - Testing (SqlConnectionString)...");

         try
         {
            AddLog(" - " + ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);

            var db = new Database("SqlConnectionString");

            string sql = @"SELECT TOP 1 * FROM V_PEOPLE_INFO_2";

            List<PeopleInfo> docList = db.Query<PeopleInfo>(sql).ToList();

            AddLog(" - SUCCESS!", LogFormat.Success);
         }
         catch (Exception ex)
         {
            AddLog(" /// ERROR /// Connection Failed!", LogFormat.ErrorBad);
            success = false;
         }
         AddLog("");

         ///////////////////////////////////////////////////
         AddLog(" - Testing (arisPublicWebDbDSN)...");

         try
         {
            AddLog(" - " + ConfigurationManager.ConnectionStrings["arisPublicWebDbDSN"].ConnectionString);

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT TOP 1 * FROM V_PEOPLE_INFO_2";

            List<PeopleInfo> docList = db.Query<PeopleInfo>(sql).ToList();

            AddLog(" - SUCCESS!", LogFormat.Success);
         }
         catch (Exception ex)
         {
            AddLog(" /// ERROR /// Connection Failed!", LogFormat.ErrorBad);
            success = false;
         }
         AddLog("");

         ///////////////////////////////////////////////////
         AddLog(" - Testing (sitePublisherDbDSN)...");

         try
         {
            AddLog(" - " + ConfigurationManager.ConnectionStrings["sitePublisherDbDSN"].ConnectionString);

            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT TOP 1 * FROM News";

            List<News> newsList = db.Query<News>(sql).ToList();

            AddLog(" - SUCCESS!", LogFormat.Success);
         }
         catch (Exception ex)
         {
            AddLog(" /// ERROR /// Connection Failed!", LogFormat.ErrorBad);
            success = false;
         }
         AddLog("");


         return success;
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

         Console.WriteLine(line);
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
         Gray
      }
   }
}
