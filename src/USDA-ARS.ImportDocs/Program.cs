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


        static void Main(string[] args)
        {

            AddLog("Getting People Sites From Umbraco...");
            PEOPLE_LIST = GetPeopleAll();
            AddLog("Done.");
            AddLog("");

            AddLog("Getting Mode Codes From Umbraco...");
            MODE_CODE_LIST = GetModeCodesAll();
            AddLog("Done.");
            AddLog("");

            AddLog("Getting Doc Folders From Umbraco...");
            DOC_FOLDER_ID_LIST = GetDocFoldersAll();
            AddLog("Done.");
            AddLog("");


            AddLog("Getting Sub Sites From Umbraco...");
            SUB_SITE_LIST = GetSubSitesAll();
            AddLog("Done.");
            AddLog("");

            AddLog("Importing Docs");
            ImportDocs();



        }


        static void ImportDocs()
        {
            // Get List of 
        }


        static List<DocFolderLookup> GetDocFoldersAll()
        {
            List<DocFolderLookup> docFoldersList = new List<DocFolderLookup>();

            if (MODE_CODE_LIST != null && MODE_CODE_LIST.Any())
            {
                foreach (ModeCodeLookup modeCodeNode in MODE_CODE_LIST)
                {
                    ApiResponse responsePage = new ApiResponse();

                    responsePage = GetCalls.GetNodeByUmbracoId(modeCodeNode.UmbracoId);

                    if (responsePage != null && true == responsePage.Success && responsePage.ContentList != null && responsePage.ContentList.Count > 0)
                    {
                        if (responsePage.ContentList[0].ChildContentList != null)
                        {
                            ApiContent docFolder = responsePage.ContentList[0].ChildContentList.Where(p => p.DocType == "DocFolder").FirstOrDefault();

                            if (docFolder != null)
                            {
                                DOC_FOLDER_ID_LIST.Add(new DocFolderLookup { ModeCode = modeCodeNode.ModeCode, UmbracoDocFolderId = docFolder.Id });
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
                            ApiProperty modeCode = node.Properties.Where(p => p.Key == "modeCode").FirstOrDefault();

                            if (modeCode != null)
                            {
                                peopleList.Add(new PersonLookup { PersonId = Convert.ToInt32(modeCode.Value), UmbracoPersonId = node.Id });

                                AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
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
