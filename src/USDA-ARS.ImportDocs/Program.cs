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

namespace USDA_ARS.ImportDocs
{
    class Program
    {
        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static List<PersonLookup> PEOPLE_LIST = null;
        static List<DocFolderLookup> DOC_FOLDER_ID_LIST = null;
        static List<SubSiteLookup> SUB_SITE_LIST = null;
        static List<AdHocFolderLookup> AD_HOC_FOLDER_LIST = null;


        static void Main(string[] args)
        {

            AddLog("Getting People Sites From Umbraco...");
            PEOPLE_LIST = GetPeopleAll();
            AddLog("Done. Count: " + PEOPLE_LIST.Count);
            AddLog("");

            AddLog("Getting Mode Codes From Umbraco...");
            MODE_CODE_LIST = GetModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_LIST.Count);
            AddLog("");

            AddLog("Getting Doc Folders From Umbraco...");
            DOC_FOLDER_ID_LIST = GetDocFoldersAll();
            AddLog("Done. Count: " + DOC_FOLDER_ID_LIST.Count);
            AddLog("");


            AddLog("Getting Sub Sites From Umbraco...");
            SUB_SITE_LIST = GetSubSitesAll();
            AddLog("Done. Count: " + SUB_SITE_LIST.Count);
            AddLog("");


            AddLog("Importing Docs");
            ImportDocs();



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









        static void AddDocToModeCode(string modeCode, ImportPage importPage)
        {
            DocFolderLookup getDocFolder = DOC_FOLDER_ID_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

            if (getDocFolder != null)
            {
                int umbracoParentId = getDocFolder.UmbracoDocFolderId;

                ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.OldDocId, importPage.OldDocType, 1);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    int umbracoId = response.ContentList[0].Id;

                    AddLog("Page added:[" + modeCode + "] (" + umbracoId + ") " + importPage.Title);

                    if (importPage.SubPages != null && importPage.SubPages.Any())
                    {
                        foreach (ImportPage subPage in importPage.SubPages)
                        {
                            ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

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
            PersonLookup getPersonSite = PEOPLE_LIST.Where(p => p.PersonId == personId).FirstOrDefault();

            if (getPersonSite != null)
            {
                int umbracoParentId = getPersonSite.UmbracoPersonId;

                ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.OldDocId, importPage.OldDocType, 1);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    int umbracoId = response.ContentList[0].Id;

                    AddLog("Page added:[" + personId + "] (" + umbracoId + ") " + importPage.Title);

                    if (importPage.SubPages != null && importPage.SubPages.Any())
                    {
                        foreach (ImportPage subPage in importPage.SubPages)
                        {
                            ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

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
            int umbracoParentId = 0;

            AdHocFolderLookup testAdHocFolder = AD_HOC_FOLDER_LIST.Where(p => p.ModeCode == modeCode && p.AdHocFolderName.ToLower() == adHocFolderName.ToLower()).FirstOrDefault();

            if (testAdHocFolder != null)
            {
                umbracoParentId = testAdHocFolder.UmbracoId;
            }
            else
            {
                testAdHocFolder = AddAdHocFolder(modeCode, adHocFolderName);

                if (testAdHocFolder != null)
                {
                    umbracoParentId = testAdHocFolder.UmbracoId;
                }
            }


            if (umbracoParentId > 0)
            {
                ApiResponse response = AddUmbracoPage(umbracoParentId, importPage.Title, importPage.BodyText, importPage.OldDocId, importPage.OldDocType, 1);

                if (response != null && response.ContentList != null && response.ContentList.Any())
                {
                    int umbracoId = response.ContentList[0].Id;

                    AddLog("Page added:[" + modeCode + "] (" + umbracoId + ") " + importPage.Title);

                    if (importPage.SubPages != null && importPage.SubPages.Any())
                    {
                        foreach (ImportPage subPage in importPage.SubPages)
                        {
                            ApiResponse subpageResponse = AddUmbracoPage(umbracoId, "Page " + subPage.PageNumber, subPage.BodyText, importPage.OldDocId, importPage.OldDocType, subPage.PageNumber);

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


        static ApiResponse AddUmbracoPage(int parentId, string name, string body, int oldId, string oldDocType, int pageNum)
        {
            ApiContent content = new ApiContent();

            string oldUrl = "";
            oldUrl = "/" + oldDocType + "/doc.htm?docid=" + oldId;

            if (pageNum > 1)
            {
                oldUrl += "&page=" + pageNum;
                name = "Page " + pageNum;
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


        static List<SubSiteLookup> GetSubSitesAll()
        {
            List<SubSiteLookup> subSitesList = new List<SubSiteLookup>();

            subSitesList.Add(new SubSiteLookup { SubSiteName = "", UmbracoId = 0 });

            return subSitesList;
        }


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
        }
    }
}
