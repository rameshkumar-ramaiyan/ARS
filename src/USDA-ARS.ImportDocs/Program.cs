﻿using System;
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
            bool forceCacheUpdate = false;

            if (args != null && args.Length == 1)
            {
                forceCacheUpdate = true;
            }

            AddLog("Getting People Sites From Umbraco...");
            GeneratePeopleList(forceCacheUpdate);
            AddLog("Done. Count: " + PEOPLE_LIST.Count);
            AddLog("");

            AddLog("Getting Mode Codes From Umbraco...");
            GenerateModeCodeList(forceCacheUpdate);
            AddLog("Done. Count: " + MODE_CODE_LIST.Count);
            AddLog("");

            AddLog("Getting Doc Folders From Umbraco...");
            GenerateDocFolderList(forceCacheUpdate);
            AddLog("Done. Count: " + DOC_FOLDER_ID_LIST.Count);
            AddLog("");

            AddLog("Getting New Mode Codes...");
            MODE_CODE_NEW_LIST = GetNewModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count);
            AddLog("");


            AddLog("Importing Docs");
            ImportDocsTemp();



        }


        static void ImportDocs()
        {
            // Get List of documents

            // LOOP THROUGH DOCUMENTS
            {
                ImportPage newPage = new ImportPage();

                newPage.OldDocId = 0; // Current SitePublisher Doc ID
                newPage.Title = "{{PAGE TITLE}}"; // Document Title
                newPage.BodyText = "{{PAGE_TEXT}}"; // Document Body Text
                newPage.OldDocType = "{{DOC TYPE}}"; // SitePublisher Doc Type
                newPage.PageNumber = 1;

                // DOES IT HAVE DOC PAGES?
                {
                    newPage.SubPages = new List<ImportPage>();

                    // LOOP THOUGH DOCUMENT PAGES (IF THERE ARE ANY)
                    {
                        newPage.SubPages.Add(new ImportPage() { PageNumber = 2, BodyText = "SUB PAGE TEXT" });
                    }
                }

                // PICK ONLY 1 OF THE 3 METHODS BELOW

                // IS IT A PAGE FOR A MODE CODE?
                AddDocToModeCode("{{MODE CODE}}", newPage);

                // IS IT A PAGE FOR A MODE CODE BUT ALSO HAS AN AD HOC?
                AddDocToAdHoc("{{MODE CODE}}", "{{AD HOC FOLDER NAME}}", newPage);

                // OR IS IT A PAGE FOR A PERSON?
                AddDocToPersonSite(0, newPage);
            }


        }

        static void ImportDocsTemp()
        {
            // Get List of documents

            //1. get all site codes and doctypes

            //DataTable dtAllDocumentIdsBasedOnDocTypeWithoutParam = new DataTable();          
            //dtAllDocumentIdsBasedOnDocTypeWithoutParam = GetAllDocumentIdsBasedOnDocTypeWithoutParam();
            // DataTable dtAllDocumentIdsBasedOnDocTypeWithParamPlace = new DataTable();
            //DataTable dtAllDocumentIdsBasedOnDocTypeWithParamAdhoc = new DataTable();
            //DataTable dtAllDocumentIdsBasedOnDocTypeWithParamPerson = new DataTable();

            //dtAllDocumentIdsBasedOnDocTypeWithParamAdhoc = GetAllDocumentIdsBasedOnDocTypeWithParam("ad_hoc");
            //dtAllDocumentIdsBasedOnDocTypeWithParamPerson = GetAllDocumentIdsBasedOnDocTypeWithParam("person");
            ////2. send to doctype sp---not require now
            List<string> list = new List<string>();
            list.Add("Place");
            list.Add("ad_hoc");
            list.Add("person");

            for (int k = 0; k < list.Count; k++) // Loop through List with for
            {
                AddLog("Looping through: " + list[k]);
                AddLog("");

                DataTable dtAllDocumentIdsBasedOnDocTypeWithParam = new DataTable();
                AddLog("Getting docs based on doc type with parameter: " + list[k]);
                dtAllDocumentIdsBasedOnDocTypeWithParam = GetAllDocumentIdsBasedOnDocTypeWithParam(list[k]);
                for (int i = 0; i < dtAllDocumentIdsBasedOnDocTypeWithParam.Rows.Count; i++)
                {
                    try
                    {
                        AddLog(list[k] + ": " + i);
                        string title = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(0).ToString();
                        string currentversion = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<int>(1).ToString();
                        string doctype = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(2).ToString();
                        string published = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(3).ToString();
                        string originSite_Type = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(4).ToString();
                        string originSite_ID = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(5).ToString();
                        //string oldURL = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(6).ToString();
                        bool displayTitle = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<bool>(7);
                        int docId = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<int>(8);
                        string adHocFolderName = string.Empty;
                        if (list[k].ToString().Trim() == "ad_hoc")
                        {
                            adHocFolderName = dtAllDocumentIdsBasedOnDocTypeWithParam.Rows[i].Field<string>(9).ToString();
                        }

                        ImportPage newPage = new ImportPage();

                        DataTable dtAllDocumentIdPagesBasedOnCurrentVersion = new DataTable();
                        DataTable newDocpagesAfterDecryption = new DataTable();
                        newDocpagesAfterDecryption.Columns.Add("DocPageNum");
                        newDocpagesAfterDecryption.Columns.Add("EncDocPage");
                        newDocpagesAfterDecryption.Columns.Add("CurrentVersion");
                        newDocpagesAfterDecryption.Columns.Add("DecDocPage");
                        DataTable newDocpagesAfterDecryption1 = new DataTable();
                        //3. send to doc pages sp

                        dtAllDocumentIdPagesBasedOnCurrentVersion = GetAllDocumentIdPagesBasedOnCurrentVersion(currentversion);

                        if (dtAllDocumentIdPagesBasedOnCurrentVersion.Rows.Count > 0)
                        {
                            AddLog(" - Found Doc: " + title);

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
                                string currentVersion = dtAllDocumentIdPagesBasedOnCurrentVersion.Rows[j].Field<int>(2).ToString();
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

                                    decString = CleanUpHtml(decString);
                                }

                                else decString = "";
                                newDocpagesAfterDecryption.Rows.Add(docpageNum, encString, currentVersion, decString);

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
                                    newPage.PageNumber = 1;
                                }
                                else
                                {
                                    AddLog(" - Adding sub page: " + docpageNum);
                                    newPage.SubPages.Add(new ImportPage() { OldDocId = docId, PageNumber = docpageNum, BodyText = decString.Trim(), DisableTitle = !displayTitle });
                                }
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
                    }
                    catch (Exception ex)
                    {
                        AddLog("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        AddLog("ERROR: " + ex.ToString());
                    }
                }


                //      return dtAllDocumentIdsBasedOnDocTypeWithoutParam;

            } // for (int k = 0; k < list.Count; k++)

            using (FileStream fs = File.Create("LOG_FILE.txt"))
            {
                // Add some text to file
                Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
                fs.Write(fileText, 0, fileText.Length);
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
                int umbracoParentId = getDocFolder.UmbracoDocFolderId;

                ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, 1);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    int umbracoId = response.ContentList[0].Id;

                    AddLog("Page added:[" + modeCode + "] (UmbId: " + umbracoId + ") " + importPage.Title);

                    if (importPage.SubPages != null && importPage.SubPages.Any())
                    {
                        foreach (ImportPage subPage in importPage.SubPages)
                        {
                            ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

                            if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                            {
                                AddLog(" - SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title);
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

                ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, 1);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    int umbracoId = response.ContentList[0].Id;

                    AddLog("Page added:[" + personId + "] (" + umbracoId + ") " + importPage.Title);

                    if (importPage.SubPages != null && importPage.SubPages.Any())
                    {
                        foreach (ImportPage subPage in importPage.SubPages)
                        {
                            ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

                            if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                            {
                                AddLog(" - SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title);
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
                AddLog("!! PERSON CANNOT BE FOUND !! (" + personId + ")");
            }
        }


        static void AddDocToAdHoc(string modeCode, string adHocFolderName, ImportPage importPage)
        {
            AddLog("Add doc to mode code with ad hoc folder: " + adHocFolderName + "...");

            int umbracoParentId = 0;

            if (modeCode.Length >= 8)
            {
                string getModeCode = "";
                int intModeCode = 0;

                if (int.TryParse(modeCode.Substring(0, 8), out intModeCode))
                {
                    getModeCode = intModeCode.ToString();

                    AdHocFolderLookup testAdHocFolder = AD_HOC_FOLDER_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(getModeCode) && p.AdHocFolderName.ToLower() == adHocFolderName.ToLower()).FirstOrDefault();

                    //if (testAdHocFolder == null)
                    //{
                    //    ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(getModeCode)).FirstOrDefault();

                    //    if (modeCodeNew != null)
                    //    {
                    //        getModeCode = modeCodeNew.ModecodeNew;
                    //        AddLog("Found Mode Code from old code: " + getModeCode + " -> " + modeCodeNew.ModecodeNew);
                    //        testAdHocFolder = AD_HOC_FOLDER_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew) && p.AdHocFolderName.ToLower() == adHocFolderName.ToLower()).FirstOrDefault();
                    //    }
                    //}

                    getModeCode = Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(getModeCode);

                    if (testAdHocFolder != null)
                    {
                        umbracoParentId = testAdHocFolder.UmbracoId;
                    }
                    else
                    {
                        testAdHocFolder = AddAdHocFolder(getModeCode, adHocFolderName);

                        if (testAdHocFolder != null)
                        {
                            umbracoParentId = testAdHocFolder.UmbracoId;

                            AD_HOC_FOLDER_LIST.Add(new AdHocFolderLookup() { ModeCode = getModeCode, AdHocFolderName = adHocFolderName, UmbracoId = umbracoParentId });

                            AddLog("Ad Hoc Folder added:[" + getModeCode + "] (" + umbracoParentId + ") " + adHocFolderName);
                        }
                    }
                }


                if (umbracoParentId > 0)
                {
                    ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, 1);

                    if (response != null && response.ContentList != null && response.ContentList.Any())
                    {
                        int umbracoId = response.ContentList[0].Id;

                        AddLog("Page added:[" + modeCode + "] (" + umbracoId + ") " + importPage.Title);

                        if (importPage.SubPages != null && importPage.SubPages.Any())
                        {
                            foreach (ImportPage subPage in importPage.SubPages)
                            {
                                ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.DisableTitle, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

                                if (subpageResponse != null && subpageResponse.ContentList != null && subpageResponse.ContentList.Any())
                                {
                                    AddLog(" - SubPage added:(" + subpageResponse.ContentList[0].Id + ") " + subPage.Title);
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
        }


        static AdHocFolderLookup AddAdHocFolder(string modeCode, string adHocFolderName)
        {
            AdHocFolderLookup newAdHocFolder = null;

            ModeCodeLookup getModeCode = MODE_CODE_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

            if (getModeCode != null)
            {
                ApiResponse response = AddUmbracoFolder(getModeCode.UmbracoId, adHocFolderName);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    newAdHocFolder = new AdHocFolderLookup();

                    newAdHocFolder.ModeCode = modeCode;
                    newAdHocFolder.AdHocFolderName = adHocFolderName;
                    newAdHocFolder.UmbracoId = response.ContentList[0].Id;
                }
            }

            return newAdHocFolder;
        }


        static ApiResponse AddUmbracoPage(int parentId, string name, string body, bool hidePageTitle, int oldId, string oldDocType, int pageNum)
        {
            ApiContent content = new ApiContent();

            string oldUrl = "";
            oldUrl = "/" + oldDocType + "/docs.htm?docid=" + oldId;

            if (pageNum > 1)
            {
                oldUrl += "&page=" + pageNum;
                name = "Page " + pageNum;
            }

            if (name.Trim().ToLower() == "index")
            {
                name = oldDocType;
            }

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = "SiteStandardWebpage";
            content.Template = "StandardWebpage";

            List<ApiProperty> properties = new List<ApiProperty>();

            body = LocationsWebApp.DL.CleanHtml.CleanUpHtml(body);

            properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
            properties.Add(new ApiProperty("oldId", oldId.ToString())); // Person's ID              
            properties.Add(new ApiProperty("oldUrl", oldUrl)); // current URL           
            properties.Add(new ApiProperty("hidePageTitle", hidePageTitle)); // hide page title

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

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

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
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

            modeCodeNewList = Umbraco.Extensions.Helpers.Aris.ModeCodesNew.GetAllNewModeCode();

            return modeCodeNewList;
        }


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LOG_FILE_TEXT += line + "\r\n";
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
        static string CleanUpHtml(string bodyText)
        {
            string output = "";

            if (false == string.IsNullOrEmpty(bodyText))
            {
                HtmlCompressor htmlCompressor = new HtmlCompressor();
                htmlCompressor.setRemoveMultiSpaces(true);
                htmlCompressor.setRemoveIntertagSpaces(true);

                bodyText = Regex.Replace(bodyText, @"https://www\.ars\.usda\.gov", "");
                bodyText = Regex.Replace(bodyText, @"http://www\.ars\.usda\.gov", "");

                bodyText = ReplaceSP2withARS(bodyText);

                output = htmlCompressor.compress(bodyText);
            }

            return output;
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

                using (FileStream fs = File.Create("doc-folder-cache.txt"))
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
            string filename = "doc-folder-cache.txt";
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

                using (FileStream fs = File.Create("mode-code-cache.txt"))
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

                using (FileStream fs = File.Create("person-cache.txt"))
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
            string filename = "person-cache.txt";
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


        static string ReplaceSP2withARS(string bodyText)
        {
            if (bodyText.ToLower().IndexOf("sp2userfiles/") >= 0)
            {
                bodyText = Regex.Replace(bodyText, "sp2UserFiles/person/", "ARSUserFiles/", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                bodyText = Regex.Replace(bodyText, "sp2UserFiles/place/", "ARSUserFiles/", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            return bodyText;
        }


    }
}
