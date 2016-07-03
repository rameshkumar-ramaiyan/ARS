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
        static List<ModeCodeFolderLookup> MODE_CODE_FOLDER_LIST = null;
        //static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;
        static List<UmbracoDocLookup> UMBRACO_DOC_LOOKUP = null;
        static List<UmbracoDocLookup> UMBRACO_PERSON_LOOKUP = null;
        static List<UmbracoDocLookup> UMBRACO_OLD_URL_LOOKUP = null;


        static List<SubSite> VALID_SITES = null;
        static List<Document> VALID_DOCS = null;

        static List<ImportedNav> IMPORTED_NAV = new List<ImportedNav>();

        static void Main(string[] args)
        {
            AddLog("-= IMPORTING NAVIGATION =-");
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
                AddLog("Getting Mode Codes From Umbraco...");
                GenerateModeCodeList(false);
                AddLog("Done. Count: " + MODE_CODE_LIST.Count);
                AddLog("");

                AddLog("Getting Mode Code Folders From Umbraco...");
                GenerateModeCodeFolderList(false);
                AddLog("Done. Count: " + MODE_CODE_FOLDER_LIST.Count);
                AddLog("");

                //AddLog("Getting New Mode Codes...");
                //MODE_CODE_NEW_LIST = GetNewModeCodesAll();
                //AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count);
                //AddLog("");

                AddLog("Getting Umbraco Docs...");
                UMBRACO_DOC_LOOKUP = GetUmbracoDocsAll();
                AddLog("Done. Count: " + UMBRACO_DOC_LOOKUP.Count);
                AddLog("");

                AddLog("Getting Umbraco People...");
                UMBRACO_PERSON_LOOKUP = GetUmbracoPeopleAll();
                AddLog("Done. Count: " + UMBRACO_PERSON_LOOKUP.Count);
                AddLog("");

                AddLog("Getting Umbraco Old Urls...");
                UMBRACO_OLD_URL_LOOKUP = GetUmbracoOldUrlAll();
                AddLog("Done. Count: " + UMBRACO_OLD_URL_LOOKUP.Count);
                AddLog("");

                AddLog("Getting Valid Sites...");
                VALID_SITES = GetValidSubSitesAll();
                AddLog("Done. Count: " + VALID_SITES.Count);
                AddLog("");

                AddLog("Getting Valid Docs...");
                VALID_DOCS = GetValidDocsAll();
                AddLog("Done. Count: " + VALID_DOCS.Count);
                AddLog("");

                List<NavSystem> navSysModeCodeList = NavSystems.GetNavModeCodeList();

                // Import the Navs
                if (navSysModeCodeList != null)
                {
                    foreach (NavSystem navSysModeCodeItem in navSysModeCodeList)
                    {
                        AddLog("ModeCode: " + navSysModeCodeItem.OriginSiteId);

                        //== PLACE
                        if (navSysModeCodeItem.OriginSiteType.ToLower() == "place")
                        {
                            List<NavSystem> filteredNavSysModeCode = NavSystems.GetNavSysListByPlace(navSysModeCodeItem.OriginSiteId, navSysModeCodeItem.OriginSiteType);

                            ModeCodeLookup modeCode = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(navSysModeCodeItem.OriginSiteId)).FirstOrDefault();

                            //if (modeCode == null)
                            //{
                            //    ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(navSysModeCodeItem.OriginSiteId)).FirstOrDefault();

                            //    if (modeCodeNew != null)
                            //    {
                            //        modeCode = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew)).FirstOrDefault();
                            //    }
                            //}


                            if (modeCode != null)
                            {
                                AddLog("Mode Code Found: " + modeCode.ModeCode + " umbId: " + modeCode.UmbracoId);

                                string json = CreateLeftNav(filteredNavSysModeCode);

                                if (false == string.IsNullOrEmpty(json))
                                {
                                    UpdateUmbracoPageNav(modeCode.UmbracoId, json);
                                }
                            }
                            else
                            {
                                AddLog("!! Mode Code NOT Found: " + navSysModeCodeItem.OriginSiteId);
                            }
                        }

                        //== PERSON
                        else if (navSysModeCodeItem.OriginSiteType.ToLower() == "person")
                        {
                            string personId = navSysModeCodeItem.OriginSiteId;
                            int umbracoPersonNodeId = 0;
                            string modeCode = "";

                            UmbracoDocLookup personUmbracoNode = UMBRACO_PERSON_LOOKUP.Where(p => p.DocId == navSysModeCodeItem.OriginSiteId).FirstOrDefault();

                            if (personUmbracoNode != null)
                            {
                                AddLog("Person Site Found: " + personUmbracoNode.DocId + " umbId: " + personUmbracoNode.UmbracoId);

                                umbracoPersonNodeId = personUmbracoNode.UmbracoId;

                                UmbracoDocLookup personParentModeNode = GetModeCodeByUmbracoPersonId(personId);

                                if (personParentModeNode != null)
                                {
                                    List<NavSystem> filteredNavSysModeCode = NavSystems.GetNavSysListByPlace(Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(personParentModeNode.DocId));

                                    if (filteredNavSysModeCode != null)
                                    {
                                        filteredNavSysModeCode = filteredNavSysModeCode.Where(p => p.BBSect.ToLower() == "pandp" && p.NavPageLoc.ToLower() == "right").ToList();

                                        if (filteredNavSysModeCode != null)
                                        {
                                            string json = CreateLeftNav(filteredNavSysModeCode);

                                            if (false == string.IsNullOrEmpty(json))
                                            {
                                                UpdateUmbracoPageNav(personUmbracoNode.UmbracoId, json);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddLog("!! Person Site NOT Found: " + navSysModeCodeItem.OriginSiteId);
                            }
                        }
                    }
                }


                // Save Nav Import




                AddLog("");
                AddLog("");
                AddLog("");

                AddLog("= Link Nav to Umbraco Nodes =");

                // Link the Navs

                //// MODE CODES
                //if (VALID_DOCS != null && MODE_CODE_LIST != null)
                //{
                //    foreach (ModeCodeLookup modeCode in MODE_CODE_LIST)
                //    {
                //        List<Document> filterDocList = VALID_DOCS.Where(p => p.OriginSiteId == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode.ModeCode)).ToList();

                //        if (filterDocList != null)
                //        {
                //            foreach (Document doc in filterDocList)
                //            {
                //                UmbracoDocLookup umbracoDoc = UMBRACO_DOC_LOOKUP.Where(p => p.DocId == doc.DocId.ToString()).FirstOrDefault();

                //                if (umbracoDoc != null)
                //                {
                //                    AddLog("Getting Umbraco Old Url... (" + umbracoDoc.UmbracoId + ")");

                //                    UmbracoDocLookup getUmbracoOldUrl = UMBRACO_OLD_URL_LOOKUP.Where(p => p.UmbracoId == umbracoDoc.UmbracoId).FirstOrDefault();

                //                    if (getUmbracoOldUrl != null)
                //                    {
                //                        NavByPage navByPage = GetNavsByProduction(getUmbracoOldUrl.OldUrl);

                //                        if (navByPage != null)
                //                        {
                //                            List<ImportedNav> importedNavList = new List<ImportedNav>();

                //                            ImportedNav foundNav = null;

                //                            if (navByPage.NavLeft > 0)
                //                            {
                //                                AddLog(" - Nav Left Found: " + navByPage.NavLeft);

                //                                foundNav = IMPORTED_NAV.Where(p => p.NavSysId == navByPage.NavLeft).FirstOrDefault();

                //                                if (foundNav != null)
                //                                {
                //                                    importedNavList.Add(foundNav);
                //                                }
                //                            }
                //                            if (navByPage.NavRight > 0)
                //                            {
                //                                AddLog(" - Nav Right Found: " + navByPage.NavRight);

                //                                foundNav = IMPORTED_NAV.Where(p => p.NavSysId == navByPage.NavRight).FirstOrDefault();

                //                                if (foundNav != null)
                //                                {
                //                                    importedNavList.Add(foundNav);
                //                                }
                //                            }


                //                            if (importedNavList != null && importedNavList.Any())
                //                            {
                //                                string jsonNav = LinkLeftNavItemsList(importedNavList);

                //                                if (false == string.IsNullOrEmpty(jsonNav))
                //                                {
                //                                    UpdateUmbracoPageLinkNav(umbracoDoc.UmbracoId, jsonNav);
                //                                }
                //                            }

                //                        }
                //                    }


                //                    ApiResponse getUmbracoNode = GetCalls.GetNodeByUmbracoId(umbracoDoc.UmbracoId);

                //                    if (getUmbracoNode != null && getUmbracoNode.ContentList != null && getUmbracoNode.ContentList.Any())
                //                    {

                //                    }


                //                    List<NavSystem> filteredNavSystemList = new List<NavSystem>();

                //                    List<NavSystem> navSystemList = NavSystems.GetNavSysListByPlace(Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode.ModeCode)).ToList();


                //                    if (true == string.IsNullOrWhiteSpace(doc.DocType))
                //                    {
                //                        doc.DocType = "Main";
                //                    }

                //                    // Checked for Related nav first
                //                    filteredNavSystemList.AddRange(navSystemList.Where(p => p.NavPageLoc.ToLower() == "related"));

                //                    if (filteredNavSystemList != null && filteredNavSystemList.Count <= 0)
                //                    {
                //                        // check for left nav
                //                        List<NavSystem> navSystemItemList = navSystemList.Where(p => p.NavPageLoc.ToLower() == "left" &&
                //                                p.BBSect.ToLower() == doc.DocType.ToLower() && p.NavSysLabel.ToLower() == "index").ToList();
                //                        if (navSystemItemList != null)
                //                        {
                //                            filteredNavSystemList.AddRange(navSystemItemList);
                //                        }


                //                        // check for right nav
                //                        List<NavSystem> navSystemItemList2 = navSystemList.Where(p => p.NavSysId == doc.RLNav).ToList();
                //                        if (navSystemItemList2 != null)
                //                        {
                //                            filteredNavSystemList.AddRange(navSystemItemList2);
                //                        }
                //                    }


                //                    if (filteredNavSystemList != null && filteredNavSystemList.Any())
                //                    {
                //                        List<ImportedNav> importedNavList = new List<ImportedNav>();

                //                        foreach (NavSystem navSys in filteredNavSystemList)
                //                        {
                //                            ImportedNav foundNav = IMPORTED_NAV.Where(p => p.NavSysId == navSys.NavSysId).FirstOrDefault();

                //                            if (foundNav != null)
                //                            {
                //                                importedNavList.Add(foundNav);
                //                            }
                //                        }


                //                        if (importedNavList != null && importedNavList.Any())
                //                        {
                //                            string jsonNav = LinkLeftNavItemsList(importedNavList);

                //                            if (false == string.IsNullOrEmpty(jsonNav))
                //                            {
                //                                UpdateUmbracoPageLinkNav(umbracoDoc.UmbracoId, jsonNav);
                //                            }
                //                        }
                //                    }
                //                }




                //                //









                //            }
                //        }
                //    }


                //    foreach (Document doc in VALID_DOCS)
                //    {
                //        AddLog("Doc: " + doc.Title + "[" + Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(doc.OriginSiteId + "]"));





                //        // Link Page Navs Next
                //    }
                //}

                if (UMBRACO_OLD_URL_LOOKUP != null)
                {
                    foreach (UmbracoDocLookup oldNode in UMBRACO_OLD_URL_LOOKUP)
                    {
                        if (oldNode != null)
                        {
                            AddLog("");

                            NavByPage navByPage = GetNavsByProduction(oldNode.OldUrl);

                            if (navByPage != null)
                            {
                                AddLog(" - Updating Nav for Umbraco Page: " + oldNode.UmbracoId);

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


                                if (importedNavList != null && importedNavList.Any())
                                {
                                    string jsonNav = LinkLeftNavItemsList(importedNavList);

                                    if (false == string.IsNullOrEmpty(jsonNav))
                                    {
                                        AddLog(" - Updating...");
                                        ApiResponse apiResponse = UpdateUmbracoPageLinkNav(oldNode.UmbracoId, jsonNav, oldNode.OldUrl);

                                        if (apiResponse != null && apiResponse.ContentList != null && apiResponse.ContentList.Count == 1)
                                        {
                                            AddLog(" - Saved: (" + apiResponse.ContentList[0].Id + ") " + apiResponse.ContentList[0].Name);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }


            }

            using (FileStream fs = File.Create("LOG_FILE.txt"))
            {
                // Add some text to file
                Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
                fs.Write(fileText, 0, fileText.Length);
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

        static List<ModeCodeLookup> GetModeCodeLookupCache()
        {
            string filename = "mode-code-cache.txt";
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

                using (FileStream fs = File.Create("mode-code-cache.txt"))
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
                                string oldUrl = "";

                                ApiProperty oldUrlProp = node.Properties.Where(p => p.Key == "oldUrl").FirstOrDefault();

                                if (oldUrlProp != null)
                                {
                                    oldUrl = oldUrlProp.Value.ToString();
                                }

                                modeCodeList.Add(new ModeCodeLookup { ModeCode = modeCode.Value.ToString(), UmbracoId = node.Id, Url = node.Url, OldUrl = oldUrl });

                                AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
                            }
                        }
                    }
                }
            }

            return modeCodeList;
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
            string filename = "mode-code-folder-cache.txt";
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

                using (FileStream fs = File.Create("mode-code-folder-cache.txt"))
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
            string filename = "nav-import-cache.txt";
            List<ImportedNav> navImportList = new List<ImportedNav>();

            if (true == File.Exists(filename))
            {
                using (StreamReader sr = File.OpenText(filename))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] lineArray = s.Split('|');

                        navImportList.Add(new ImportedNav() { NavSysId = Convert.ToInt32(lineArray[0]), UmbracoGuid = Guid.Parse(lineArray[1]), NavTitle = lineArray[2], Section = lineArray[3], Label = lineArray[4] });
                    }
                }
            }

            return navImportList;
        }


        static List<ImportedNav> CreateNavImportLookupCache()
        {
            List<ImportedNav> navImportList = new List<ImportedNav>();

            navImportList = IMPORTED_NAV;

            StringBuilder sb = new StringBuilder();

            if (navImportList != null)
            {
                foreach (ImportedNav navImportItem in navImportList)
                {
                    sb.AppendLine(navImportItem.NavSysId + "|" + navImportItem.UmbracoGuid + "|" + navImportItem.NavTitle + "|" + navImportItem.Section + "|" + navImportItem.Label);
                }

                using (FileStream fs = File.Create("nav-import-cache.txt"))
                {
                    // Add some text to file
                    Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(fileText, 0, fileText.Length);
                }
            }

            return navImportList;
        }



        static List<ModeCodeNew> GetNewModeCodesAll()
        {
            List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

            modeCodeNewList = Umbraco.Extensions.Helpers.Aris.ModeCodesNew.GetAllNewModeCode();

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


        static string CreateLeftNav(List<NavSystem> navSysList)
        {
            string output = "";

            if (navSysList != null && navSysList.Any())
            {
                // USED FOR ALL ARCHETYPE DATA TYPES
                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                ApiArchetype navArchetypeItem = new ApiArchetype();

                //{"fieldsets":[{"properties":[{"alias":"customLeftNavTitle","value":"Hello TEST"},{"alias":"customLeftNav","value":"{\"fieldsets\":[{\"properties\":[{\"alias\":\"sectionTitle\",\"value\":\"Header TEST\"}],\"alias\":\"topicsHeader\",\"disabled\":false,\"id\":\"0e7e6013-492a-4e87-80b5-57b93c13ac62\"},{\"properties\":[{\"alias\":\"title\",\"value\":\"Link TEST\"},{\"alias\":\"location\",\"value\":\"[\\r\\n  {\\r\\n    \\\"name\\\": \\\"/test/\\\",\\r\\n    \\\"url\\\": \\\"/test/\\\",\\r\\n    \\\"icon\\\": \\\"icon-link\\\"\\r\\n  }\\r\\n]\"}],\"alias\":\"topicsItem\",\"disabled\":false,\"id\":\"c7b5f8b5-e91b-40f6-bb56-067eb4082599\"}]}"}],"alias":"leftNavCustom","disabled":false,"id":"2a36fb59-0348-4819-8c78-9a178e1d9b29"},{"properties":[{"alias":"customLeftNavTitle","value":"Hello TEST 2"},{"alias":"customLeftNav","value":"{\"fieldsets\":[{\"properties\":[{\"alias\":\"sectionTitle\",\"value\":\"Header TEST 2\"}],\"alias\":\"topicsHeader\",\"disabled\":false,\"id\":\"6c86fb74-e608-4afd-993d-da3d5864a1a4\"},{\"properties\":[{\"alias\":\"title\",\"value\":\"Link TEST 2\"},{\"alias\":\"location\",\"value\":\"[\\r\\n  {\\r\\n    \\\"name\\\": \\\"/test/\\\",\\r\\n    \\\"url\\\": \\\"/test2/\\\",\\r\\n    \\\"icon\\\": \\\"icon-link\\\"\\r\\n  }\\r\\n]\"}],\"alias\":\"topicsItem\",\"disabled\":false,\"id\":\"ddd4daf3-9326-4fbd-9abc-4b85fbd22c01\"}]}"}],"alias":"leftNavCustom","disabled":false,"id":"f470a633-33b2-48da-9fc2-489ef2f9a485"}]}

                navArchetypeItem.Fieldsets = new List<Fieldset>();

                foreach (NavSystem navSysItem in navSysList)
                {
                    bool doImport = true;

                    if (navSysItem.OriginSiteType.ToLower() == "person")
                    {
                        // do import
                    }

                    else if (navSysItem.OriginSiteType.ToLower() == "place")
                    {

                        if (false == string.IsNullOrEmpty(navSysItem.NavPageLoc))
                        {
                            navSysItem.NavPageLoc = navSysItem.NavPageLoc.Trim().ToLower();
                        }

                        SubSite testSubSite = VALID_SITES.Where(p => p.SiteCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(navSysItem.OriginSiteId)).FirstOrDefault();

                        if (testSubSite == null)
                        {
                            doImport = false;
                        }

                        if (true == doImport && navSysItem.NavPageLoc == "right")
                        {
                            Document testDoc = VALID_DOCS.Where(p => p.RLNav == navSysItem.NavSysId).FirstOrDefault();

                            if (testDoc == null)
                            {
                                doImport = false;
                            }
                        }
                    }

                    if (true == doImport)
                    {
                        List<Navigation> navItemsList = Navigations.GetNavigationList(navSysItem.NavSysId);
                        string archetypeNavItemsList = CreateLeftNavItemsList(navItemsList);

                        if (navItemsList != null && navItemsList.Any() && false == string.IsNullOrEmpty(archetypeNavItemsList))
                        {
                            // LOOP START
                            Fieldset fieldsetNav = new Fieldset();

                            fieldsetNav.Alias = "leftNavCustom";
                            fieldsetNav.Disabled = false;
                            fieldsetNav.Id = Guid.NewGuid();
                            fieldsetNav.Properties = new List<Property>();

                            if (true == string.IsNullOrWhiteSpace(navSysItem.BBSect))
                            {
                                navSysItem.BBSect = "Main";
                            }

                            string navTitle = navSysItem.BBSect + " - " + navSysItem.NavSysLabel;

                            fieldsetNav.Properties.Add(new Property("customLeftNavTitle", navTitle));

                            fieldsetNav.Properties.Add(new Property("customLeftNav", archetypeNavItemsList));

                            navArchetypeItem.Fieldsets.Add(fieldsetNav);

                            IMPORTED_NAV.Add(new ImportedNav { NavSysId = navSysItem.NavSysId, UmbracoGuid = fieldsetNav.Id, NavTitle = navTitle, Section = navSysItem.BBSect, Label = navSysItem.NavSysLabel });

                            // LOOP END

                        }
                    }

                    if (navArchetypeItem.Fieldsets != null && navArchetypeItem.Fieldsets.Count > 0)
                    {
                        output = JsonConvert.SerializeObject(navArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                    }
                }
            }

            return output;
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
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("/pandp/people/people.htm?personid=", "/people-locations/person?person-id=");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("http://www.ars.usda.gov", "");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("http://ars.usda.gov", "");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("/sp2userfiles/place", "/ARSUserFiles");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("/sp2userfiles/people", "/ARSUserFiles");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("/sp2userfiles/person", "/ARSUserFiles");

                            fieldsetTopic.Alias = "topicsItem";
                            fieldsetTopic.Disabled = false;
                            fieldsetTopic.Id = Guid.NewGuid();
                            fieldsetTopic.Properties = new List<Property>();

                            fieldsetTopic.Properties.Add(new Property("title", navItem.NavLabel));

                            string urlNav = navItem.NavURL.ToLower();

                            if (urlNav.IndexOf("/arsuserfiles/") >= 0 && (urlNav.EndsWith(".pdf") || urlNav.EndsWith(".doc") || urlNav.EndsWith(".jpg") || urlNav.EndsWith(".xls") ||
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
                    // LOOP START
                    Fieldset fieldsetNav = new Fieldset();

                    fieldsetNav.Alias = "leftNavPicker";
                    fieldsetNav.Disabled = false;
                    fieldsetNav.Id = Guid.NewGuid();
                    fieldsetNav.Properties = new List<Property>();

                    Dictionary dictionary = new Dictionary(nav.UmbracoGuid.ToString(), nav.NavTitle);
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


        static ApiResponse UpdateUmbracoPageNav(int id, string leftNav)
        {
            ApiContent content = new ApiContent();

            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("leftNavCreate", leftNav)); // 

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }


        static ApiResponse UpdateUmbracoPageLinkNav(int id, string leftNavJson, string oldUrl)
        {
            ApiContent content = new ApiContent();

            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("leftNavPicker", leftNavJson)); // 

            if (true == oldUrl.ToLower().StartsWith("/pandp/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "dbc6a2c8-1e09-4f23-9a91-5823a9729626")); // 
            }
            else if (true == oldUrl.ToLower().StartsWith("/research/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "5f5e3831-ce78-4f8b-92cc-81e1857f6bd7")); // 
            }
            else if (true == oldUrl.ToLower().StartsWith("/careers/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "d5f6c79c-a25c-4838-b154-20398f0685e2")); // 
            }
            else if (true == oldUrl.ToLower().StartsWith("/aboutus/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "ba07e133-9703-42a6-87aa-8196ed8dc55d")); // 
            }
            else if (true == oldUrl.ToLower().StartsWith("/news/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "ac79b700-ad67-4179-a4f6-db9705ecce31")); // 
            }
            else if (true == oldUrl.ToLower().StartsWith("/services/"))
            {
                properties.Add(new ApiProperty("navigationCategory", "6dded150-790b-4ddb-b0dd-15674a066840")); // 
            }


            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }


        static NavByPage GetNavsByProduction(string url)
        {
            NavByPage navByPage = null;

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
                            navByPage = new NavByPage();

                            navByPage.NavLeft = findLeftSysId;
                            navByPage.NavRight = findRightSysId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("!!! Can't get website page. !!!" + url);
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

                AddLog(" - SUCCESS!");
            }
            catch (Exception ex)
            {
                AddLog(" /// ERROR /// Connection Failed!");
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

                AddLog(" - SUCCESS!");
            }
            catch (Exception ex)
            {
                AddLog(" /// ERROR /// Connection Failed!");
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

                AddLog(" - SUCCESS!");
            }
            catch (Exception ex)
            {
                AddLog(" /// ERROR /// Connection Failed!");
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

                AddLog(" - SUCCESS!");
            }
            catch (Exception ex)
            {
                AddLog(" /// ERROR /// Connection Failed!");
                success = false;
            }
            AddLog("");


            return success;
        }


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LOG_FILE_TEXT += line + "\r\n";
        }
    }
}
