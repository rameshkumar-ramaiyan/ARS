using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

        static List<Document> VALID_DOCS = null;

        static void Main(string[] args)
        {
            AddLog("Getting Mode Codes From Umbraco...");
            GenerateModeCodeList(false);
            AddLog("Done. Count: " + MODE_CODE_LIST.Count);
            AddLog("");

            AddLog("Getting New Mode Codes...");
            MODE_CODE_NEW_LIST = GetNewModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count);
            AddLog("");


            List<NavSystem> navSysModeCodeList = NavSystems.GetNavModeCodeList();


            if (navSysModeCodeList != null)
            {
                foreach (NavSystem navSysModeCodeItem in navSysModeCodeList)
                {
                    AddLog("ModeCode: " + navSysModeCodeItem.OriginSiteId);

                    List<NavSystem> filteredNavSysModeCode = NavSystems.GetNavSysListByPlace(navSysModeCodeItem.OriginSiteId);

                    ModeCodeLookup modeCode = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(navSysModeCodeItem.OriginSiteId)).FirstOrDefault();

                    if (modeCode == null)
                    {
                        ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(navSysModeCodeItem.OriginSiteId)).FirstOrDefault();

                        if (modeCodeNew != null)
                        {
                            modeCode = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew)).FirstOrDefault();
                        }
                    }


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
                                modeCodeList.Add(new ModeCodeLookup { ModeCode = modeCode.Value.ToString(), UmbracoId = node.Id, Url = node.Url });

                                AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
                            }
                        }
                    }
                }
            }

            return modeCodeList;
        }


        static List<ModeCodeNew> GetNewModeCodesAll()
        {
            List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

            modeCodeNewList = Umbraco.Extensions.Helpers.Aris.ModeCodesNew.GetAllNewModeCode();

            return modeCodeNewList;
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
                    List<Navigation> navItemsList = Navigations.GetNavigationList(navSysItem.NavSysId);
                    string archetypeNavItemsList = CreateLeftNavItemsList(navItemsList);

                    if (navItemsList != null && navItemsList.Any() && false == string.IsNullOrEmpty(archetypeNavItemsList))
                    {
                        // LOOP START
                        Fieldset fieldsetTopic = new Fieldset();

                        fieldsetTopic.Alias = "leftNavCustom";
                        fieldsetTopic.Disabled = false;
                        fieldsetTopic.Id = Guid.NewGuid();
                        fieldsetTopic.Properties = new List<Property>();

                        if (true == string.IsNullOrWhiteSpace(navSysItem.BBSect))
                        {
                            navSysItem.BBSect = "Main";
                        }

                        fieldsetTopic.Properties.Add(new Property("customLeftNavTitle", navSysItem.BBSect + " - " + navSysItem.NavSysLabel));

                        fieldsetTopic.Properties.Add(new Property("customLeftNav", archetypeNavItemsList));

                        navArchetypeItem.Fieldsets.Add(fieldsetTopic);
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
                    if (navItem != null && (false == string.IsNullOrWhiteSpace(navItem.NavURL) || string.IsNullOrWhiteSpace(navItem.NavLabel)))
                    {
                        // LOOP START
                        Fieldset fieldsetTopic = new Fieldset();

                        if (true == string.IsNullOrWhiteSpace(navItem.NavURL))
                        {
                            fieldsetTopic.Alias = "topicsHeader";
                            fieldsetTopic.Disabled = false;
                            fieldsetTopic.Id = Guid.NewGuid();
                            fieldsetTopic.Properties = new List<Property>();
                            fieldsetTopic.Properties.Add(new Property("sectionTitle", navItem.NavLabel));
                        }
                        else
                        {
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("/pandp/people/people.htm?personid=", "/people-locations/person?person-id=");
                            navItem.NavURL = navItem.NavURL.ToLower().Replace("http://www.ars.usda.gov", "");
                            navItem.NavURL = navItem.NavURL.Replace("/sp2userfiles/place", "/ARSUserFiles");

                            fieldsetTopic.Alias = "topicsItem";
                            fieldsetTopic.Disabled = false;
                            fieldsetTopic.Id = Guid.NewGuid();
                            fieldsetTopic.Properties = new List<Property>();

                            fieldsetTopic.Properties.Add(new Property("title", navItem.NavLabel));

                            Link navLink = new Link(navItem.NavURL, navItem.NavURL, ""); // set the url path
                            fieldsetTopic.Properties.Add(new Property("location", "[" + JsonConvert.SerializeObject(navLink, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));
                        }

                        navArchetypeItem.Fieldsets.Add(fieldsetTopic);
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


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LOG_FILE_TEXT += line + "\r\n";
        }
    }
}
