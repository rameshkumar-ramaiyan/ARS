﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using USDA_ARS.ImportInfoStaff.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportInfoStaff
{
    class Program
    {
        static string LOG_FILE_TEXT = "";
        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        static int UMBRACO_START_NODE = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Umbraco:StartingNodeId"));
        static string UMBRACO_NAV_CATEGORY_GUID = ConfigurationManager.AppSettings.Get("Umbraco:NavCategoryGuid");
        static string INFO_STAFF_PATH = ConfigurationManager.AppSettings.Get("InfoStaff:Path");
        static string INFO_STAFF_URL = ConfigurationManager.AppSettings.Get("InfoStaff:Url");
        static string INFO_STAFF_FILE_URL = ConfigurationManager.AppSettings.Get("InfoStaff:FileUrl");

        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static List<InfoStaffPathLookup> PATH_LOOKUP_LIST = new List<InfoStaffPathLookup>();

        static void Main(string[] args)
        {
            bool forceCacheUpdate = false;

            if (args != null && args.Length == 1)
            {
                forceCacheUpdate = true;
            }

            AddLog("Getting Mode Codes From Umbraco...");
            GenerateModeCodeList(forceCacheUpdate);
            AddLog("Done. Count: " + MODE_CODE_LIST.Count);
            AddLog("");

            PATH_LOOKUP_LIST.Add(new InfoStaffPathLookup() { Path = INFO_STAFF_PATH, UmbracoId = UMBRACO_START_NODE });

            List<string> firstFileList = Directory.GetFiles(INFO_STAFF_PATH, "*").Where(s => s.ToLower().EndsWith(".htm") || s.ToLower().EndsWith(".html")).ToList();

            string tempDir = INFO_STAFF_PATH;

            foreach (string firstFile in firstFileList)
            {
                PageImport pageImport = GetPageData(firstFile);

                if (pageImport != null)
                {
                    int pageId = AddUmbracoPage(UMBRACO_START_NODE, pageImport);
                }
                else
                {
                    AddLog("** WARNING: Page not added: " + firstFile);
                }
            }

            List<string> dirList = Directory.GetDirectories(INFO_STAFF_PATH, "*", SearchOption.AllDirectories).OrderBy(p => p).ToList();

            int parentNodeId = UMBRACO_START_NODE;

            foreach (string dir in dirList)
            {
                AddLog("DIR: " + dir);

                InfoStaffPathLookup getPathLookup = PATH_LOOKUP_LIST.Where(p => p.Path == dir).FirstOrDefault();

                if (getPathLookup == null)
                {
                    //Create Umbraco Doc Folder
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    string title = dirInfo.Name;

                    // Creates a TextInfo based on the "en-US" culture.
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    // Changes a string to titlecase.
                    title = textInfo.ToTitleCase(title);

                    int parentNodeIdParent = GetParentDirId(dir);

                    parentNodeId = AddUmbracoFolder(parentNodeIdParent, title);

                    if (parentNodeId <= 0)
                    {
                        throw new Exception("Invalid umbraco id returned.");
                    }
                    else
                    {
                        PATH_LOOKUP_LIST.Add(new InfoStaffPathLookup() { Path = dir, UmbracoId = parentNodeId });
                    }


                    // Look for index page
                    List<string> indexList = Directory.GetFiles(dir, "*.*").Where(s => s.ToLower().EndsWith("\\index.html") || s.ToLower().EndsWith("\\index.htm")).ToList();

                    if (indexList != null && indexList.Count > 0)
                    {
                        // Create Index page in Umbraco
                        PageImport pageImport = GetPageData(indexList[0]);

                        if (pageImport != null)
                        {
                            int pageId = AddUmbracoPage(parentNodeId, pageImport);

                            if (pageId <= 0)
                            {
                                throw new Exception("Invalid umbraco id returned.");
                            }
                            else
                            {
                                UpdateUmbracoPageRedirect(parentNodeId, pageId);
                            }
                        }
                        else
                        {
                            AddLog("** WARNING: Page not added: " + indexList[0]);
                        }
                    }
                }
                else
                {
                    parentNodeId = getPathLookup.UmbracoId;
                }

                tempDir = dir;

                List<string> fileList = Directory.GetFiles(dir, "*.*").Where(s => s.ToLower().EndsWith(".htm") || s.ToLower().EndsWith(".html")).ToList();

                foreach (string file in fileList)
                {
                    if (false == file.ToLower().EndsWith("\\index.html") && false == file.ToLower().EndsWith("\\index.htm"))
                    {
                        PageImport pageImport = GetPageData(file);
                        if (pageImport != null)
                        {
                            int pageId = AddUmbracoPage(parentNodeId, pageImport);
                        }
                        else
                        {
                            AddLog("** WARNING: Page not added: " + file);
                        }
                    }
                }

                AddLog("");
            }


            using (FileStream fs = File.Create("LOG_FILE.txt"))
            {
                // Add some text to file
                Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
                fs.Write(fileText, 0, fileText.Length);
            }
        }


        static int GetParentDirId(string dir)
        {
            int parentNodeIdParent = 0;

            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            InfoStaffPathLookup getPathLookupParent = PATH_LOOKUP_LIST.Where(p => p.Path == dirInfo.Parent.FullName).FirstOrDefault();

            if (getPathLookupParent != null)
            {
                parentNodeIdParent = getPathLookupParent.UmbracoId;
            }
            else
            {
                throw new Exception("Could not find parent directory.");
            }

            return parentNodeIdParent;
        }


        static PageImport GetPageData(string path)
        {
            PageImport pageImport = null;

            var fileContents = File.ReadAllText(path);

            string title = "";
            string bodyText = "";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(fileContents);

            if (doc.DocumentNode.SelectSingleNode("/html/head/title") != null)
            {
                title = doc.DocumentNode.SelectSingleNode("/html/head/title").InnerHtml;

                string[] titleArray = title.Split('/');

                if (titleArray != null && titleArray.Length > 0)
                {
                    title = titleArray[0].Trim();
                }
                if (title.ToLower() == "title")
                {
                    title = "";
                }
            }

            if (true == string.IsNullOrEmpty(title))
            {
                if (doc.DocumentNode.SelectSingleNode("/html/body/h3") != null)
                {
                    title = doc.DocumentNode.SelectSingleNode("/html/body/h3").InnerHtml;
                }
            }

            if (true == string.IsNullOrEmpty(title))
            {
                if (doc.DocumentNode.SelectSingleNode("/html/body/h3") != null)
                {
                    title = doc.DocumentNode.SelectSingleNode("/html/body/h3").InnerHtml;
                }
            }

            if (true == string.IsNullOrEmpty(title))
            {
                if (doc.DocumentNode.SelectSingleNode("/h1/font") != null)
                {
                    title = doc.DocumentNode.SelectSingleNode("/h1/font").InnerHtml;
                }
            }

            if (true == string.IsNullOrWhiteSpace(title))
            {
                FileInfo fileInfo = new FileInfo(path);

                if (fileInfo != null)
                {
                    title = fileInfo.Name.Replace(fileInfo.Extension, "");
                    // Creates a TextInfo based on the "en-US" culture.
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    // Changes a string to titlecase.
                    title = textInfo.ToTitleCase(title);
                }
                else
                {
                    title = path;
                }
            }

            if (doc.DocumentNode.SelectSingleNode("/html/body") != null)
            {
                bodyText = doc.DocumentNode.SelectSingleNode("/html/body").InnerHtml;
            }
            else
            {
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                Regex regx = new Regex("<body>(?<theBody>.*)</body>", options);

                Match match = regx.Match(bodyText);

                if (match.Success)
                {
                    bodyText = match.Groups["theBody"].Value;
                }
                else
                {
                    bodyText = fileContents;
                }
            }

            bodyText = UpdateHtml(bodyText, Path.GetDirectoryName(path));

            if (false == string.IsNullOrEmpty(bodyText) || false == string.IsNullOrWhiteSpace(title))
            {
                pageImport = new PageImport();

                string oldPath = path;

                oldPath = oldPath.Replace(INFO_STAFF_PATH, "/is");
                oldPath = oldPath.Replace("\\", "/");

                pageImport.Title = title;
                pageImport.BodyText = bodyText;
                pageImport.OldPath = oldPath;
            }

            return pageImport;
        }


        static int AddUmbracoPage(int parentId, PageImport pageImport)
        {
            int umbracoId = 0;

            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = pageImport.Title;
            content.ParentId = parentId;
            content.DocType = "SiteStandardWebpage";
            content.Template = "StandardWebpage";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", pageImport.BodyText)); // HTML of person site
            properties.Add(new ApiProperty("navigationCategory", UMBRACO_NAV_CATEGORY_GUID)); // Nav Category Guid
            properties.Add(new ApiProperty("oldUrl", pageImport.OldPath)); // Old Path 
            properties.Add(new ApiProperty("hidePageTitle", true));

            if (pageImport.Title == "ARS: Photo Images")
            {
                properties.Add(new ApiProperty("umbracoRedirect", 1145)); // Redirect
            }

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            if (responseBack != null && responseBack.ContentList != null && responseBack.ContentList.Count > 0)
            {
                umbracoId = responseBack.ContentList[0].Id;

                AddLog("PAGE ADDED: " + pageImport.Title + " | " + pageImport.OldPath);
            }
            else
            {
                AddLog("!! ERROR: Could not add page: " + pageImport.Title);
            }

            return umbracoId;
        }


        static ApiResponse UpdateUmbracoPageRedirect(int id, int umbracoRedirect)
        {
            ApiContent content = new ApiContent();

            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("umbracoRedirect", umbracoRedirect)); // Contact Category Info

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }


        static int AddUmbracoFolder(int parentId, string name)
        {
            int umbracoId = 0;

            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = "DocsFolder";
            content.Template = "StandardPageList";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("navigationCategory", UMBRACO_NAV_CATEGORY_GUID)); // Nav Category Guid

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            if (responseBack != null && responseBack.ContentList != null && responseBack.ContentList.Count > 0)
            {
                umbracoId = responseBack.ContentList[0].Id;

                AddLog("FOLDER ADDED: " + name);
            }
            else
            {
                AddLog("!! ERROR: Could not add folder: " + name);
                if (responseBack != null)
                {
                    AddLog("!! ERROR: Success: " + responseBack.Success);
                    AddLog("!! ERROR: Message: " + responseBack.Message);
                }
                else
                {
                    AddLog("!! ERROR: Response empty");
                }
            }

            return umbracoId;
        }


        static string UpdateHtml(string bodyText, string oldPath)
        {
            bodyText = LocationsWebApp.DL.CleanHtml.CleanUpHtml(bodyText);

            bodyText = bodyText.Replace("http://www.ars.usda.gov", "");
            bodyText = bodyText.Replace("/pandp/people/people.htm?personid=", "/people-locations/person/?person-id=");

            // Find Mode Code Links
            MatchCollection m1 = Regex.Matches(bodyText, @"/main/site_main\.htm\?modecode=([\d\-]*)", RegexOptions.Singleline);

            if (MODE_CODE_LIST != null && MODE_CODE_LIST.Any())
            {
                foreach (Match m in m1)
                {
                    string modeCode = m.Groups[1].Value;

                    // Get the umbraco page by the mode code (Region/Area or Research Unit)
                    ModeCodeLookup getModeCode = MODE_CODE_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

                    if (getModeCode != null)
                    {
                        bodyText = bodyText.Replace(m.Groups[0].Value, "/{localLink:" + getModeCode.UmbracoId + "}");
                    }
                }
            }

            // Find Relative Paths
            MatchCollection m2 = Regex.Matches(bodyText, @"(src|href)=""[^""]*", RegexOptions.Singleline);

            foreach (Match m in m2)
            {
                string path = m.Groups[0].Value.Replace(m.Groups[1].Value + "=\"", "");
                string newPath = "";

                if (false == string.IsNullOrEmpty(path))
                {
                    if (true == path.IndexOf("file:///F|") >= 0)
                    {
                        newPath = path.Replace("file:///F|", "");

                        if (true == path.EndsWith(".htm") || true == path.EndsWith(".html"))
                        {
                            newPath = "/is" + newPath;
                        }
                        else
                        {
                            newPath = INFO_STAFF_FILE_URL + newPath;
                        }

                        if (newPath.IndexOf("../") >= 0)
                        {
                            newPath = FixRelativePath(newPath);
                        }
                    }
                    else if (false == path.StartsWith("/") && false == path.StartsWith("http") && false == path.StartsWith("#") && false == path.StartsWith("mailto:"))
                    {
                        oldPath = oldPath.Replace(INFO_STAFF_PATH, "");
                        oldPath = oldPath.Replace("\\", "/");

                        newPath = oldPath + "/" + path;

                        if (true == m.Groups[0].Value.EndsWith(".htm") || true == m.Groups[0].Value.EndsWith(".html") || m.Groups[0].Value.IndexOf(".htm#") >= 0)
                        {
                            newPath = "/is" + newPath;
                        }
                        else
                        {
                            newPath = INFO_STAFF_FILE_URL + newPath;
                        }

                        //m.Groups[0].Value   "href=\"../../../index.html"    string
                        //newPath "/is/AR/archive_Nacny/apr97/../../../index.html"    string
                    }
                    else if (true == path.ToLower().StartsWith("/is"))
                    {
                        if (m.Groups[1].Value.ToLower() == "href")
                        {
                            newPath = path;
                        }
                        else
                        {
                            newPath = path.ToLower().Replace("/is", INFO_STAFF_FILE_URL);
                        }
                    }
                    else if (true == path.ToLower().StartsWith("/sp2userfiles/place"))
                    {
                        newPath = path.ToLower().Replace("/sp2userfiles/place", INFO_STAFF_FILE_URL);
                    }

                    if (false == string.IsNullOrEmpty(newPath))
                    {
                        if (newPath.IndexOf("../") >= 0)
                        {
                            newPath = FixRelativePath(newPath);
                        }

                        if (m.Groups[1].Value.ToLower() == "href")
                        {
                            if (false == newPath.ToLower().EndsWith(".html") && false == newPath.ToLower().EndsWith(".htm") && false == newPath.ToLower().EndsWith("/") && newPath.IndexOf(".htm#") < 0 && newPath.IndexOf(".html#") < 0 && newPath.IndexOf("/#") < 0)
                            {
                                newPath = path.ToLower().Replace("/is", INFO_STAFF_FILE_URL);
                            }
                        }

                        bodyText = bodyText.Replace(m.Groups[0].Value, m.Groups[1].Value + "=\"" + newPath);
                    }
                }
            }

            return bodyText;
        }


        static string FixRelativePath(string path)
        {
            List<string> splitPath = path.Split(new[] { "../" }, StringSplitOptions.None).ToList();

            if (splitPath.Count > 1)
            {
                List<string> splitUrl = splitPath[0].Split('/').ToList();

                string newPath = "";

                for (int i = 0; i < splitUrl.Count - splitPath.Count; i++)
                {
                    if (splitUrl[i] != "")
                    {
                        newPath += "/" + splitUrl[i];
                    }
                }

                newPath = newPath + "/" + splitPath[splitPath.Count - 1];

                path = newPath;
            }

            return path;
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


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LOG_FILE_TEXT += line + "\r\n";
        }
    }
}
