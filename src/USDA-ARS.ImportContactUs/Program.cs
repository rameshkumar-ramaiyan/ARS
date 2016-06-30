using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportContactUs.Models;
using USDA_ARS.ImportContactUs.Models.Aris;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportContactUs
{
    class Program
    {
        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static List<SiteUserRole> SITE_USER_ROLE_LIST = null;
        static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

        static void Main(string[] args)
        {
            AddLog("Getting Mode Codes From Umbraco...");
            MODE_CODE_LIST = GetModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_LIST.Count);
            AddLog("");

            AddLog("Getting Site User Roles From SP2...");
            SITE_USER_ROLE_LIST = GetSiteUserRolesAll();
            AddLog("Done. Count: " + SITE_USER_ROLE_LIST.Count);
            AddLog("");


            AddLog("Getting New Mode Codes...");
            MODE_CODE_NEW_LIST = GetNewModeCodesAll();
            AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count);
            AddLog("");


            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

            //{"fieldsets":[{"properties":[{"alias":"category","value":"Test Category"},{"alias":"customEmail","value":"test@axial.agency"},{"alias":"contactPerson","value":"43201"}],"alias":"siteContactInfo","disabled":false,"id":"d6016e38-7321-48b3-8695-48ab8b5b4e46"}]}

            List<SiteUserRole> distinctSiteUserRoleList = SITE_USER_ROLE_LIST.DistinctBy(p => p.SiteCode).ToList();


            if (distinctSiteUserRoleList != null && distinctSiteUserRoleList.Any())
            {
                foreach (SiteUserRole siteCode in distinctSiteUserRoleList)
                {
                    string modeCodeFix = USDA_ARS.Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(siteCode.SiteCode);
                    if (false == string.IsNullOrEmpty(modeCodeFix))
                    {
                        ModeCodeLookup getModeCode = MODE_CODE_LIST.Where(p => p.ModeCode == modeCodeFix).FirstOrDefault();

                        //if (getModeCode == null)
                        //{
                        //    string newModeCode = Umbraco.Extensions.Helpers.Aris.ModeCodesNew.GetNewModeCode(modeCodeFix);

                        //    if (false == string.IsNullOrEmpty(newModeCode))
                        //    {
                        //        newModeCode = USDA_ARS.Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(newModeCode);

                        //        getModeCode = MODE_CODE_LIST.Where(p => p.ModeCode == newModeCode).FirstOrDefault();
                        //    }
                        //}

                        if (getModeCode != null)
                        {
                            List<SiteUserRole> filteredSiteUserRoleList = SITE_USER_ROLE_LIST.Where(p => p.SiteCode == siteCode.SiteCode).ToList();

                            if (filteredSiteUserRoleList != null && filteredSiteUserRoleList.Any())
                            {
                                ApiResponse response = GetCalls.GetNodeByUmbracoId(getModeCode.UmbracoId);

                                if (response != null && response.ContentList != null && response.ContentList.Count > 0)
                                {
                                    ApiArchetype archetypeItem = new ApiArchetype();

                                    archetypeItem.Fieldsets = new List<Fieldset>();

                                    foreach (SiteUserRole user in filteredSiteUserRoleList)
                                    {
                                        // LOOP START
                                        Fieldset fieldsetCat = new Fieldset();

                                        fieldsetCat.Alias = "siteContactInfo";
                                        fieldsetCat.Disabled = false;
                                        fieldsetCat.Id = Guid.NewGuid();
                                        fieldsetCat.Properties = new List<Property>();

                                        fieldsetCat.Properties.Add(new Property("category", user.SiteRole));
                                        if (user.PersonID > 0)
                                        {
                                            fieldsetCat.Properties.Add(new Property("customEmail", ""));
                                        }
                                        else
                                        {
                                            fieldsetCat.Properties.Add(new Property("customEmail", user.Email));
                                        }
                                        fieldsetCat.Properties.Add(new Property("contactPerson", user.PersonID.ToString()));

                                        archetypeItem.Fieldsets.Add(fieldsetCat);
                                        // LOOP END
                                    }

                                    string contactCategory = JsonConvert.SerializeObject(archetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);

                                    ApiResponse responseUpdate = UpdateUmbracoPage(getModeCode.UmbracoId, contactCategory);

                                    if (responseUpdate != null && responseUpdate.ContentList != null && responseUpdate.ContentList.Count > 0)
                                    {
                                        AddLog("Page Updated: (" + responseUpdate.ContentList[0].Name + ")");
                                    }
                                    else
                                    {
                                        AddLog("!! Page NOT Updated: (" + getModeCode.ModeCode + ")");
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddLog("!! Mode Code not found. (" + modeCodeFix + ")");
                        }
                    }
                    else
                    {
                        AddLog("!! Mode Code not valid. (" + siteCode.SiteCode + ")");
                    }
                }
            }
        }


        static ApiResponse UpdateUmbracoPage(int id, string contactInfo)
        {
            ApiContent content = new ApiContent();

            content.Id = id;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("contactCategory", contactInfo)); // Contact Category Info

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }


        static List<SiteUserRole> GetSiteUserRolesAll()
        {
            List<SiteUserRole> siteUserRolesList = new List<SiteUserRole>();

            var db = new Database("sitePublisherDbDSN");

            string sql = @"select SiteUserRoles.siteRole, People.PersonID, People.EMail, siteUserRoles.site_code
                        from SiteUserRoles, 
                        People
                        where siteuserroles.Person_id = people.PersonID
                        order by Site_Code";

            siteUserRolesList = db.Query<SiteUserRole>(sql).ToList();

            return siteUserRolesList;
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

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT * FROM NewModecodes";

            modeCodeNewList = db.Query<ModeCodeNew>(sql).ToList();

            return modeCodeNewList;
        }



        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
        }
    }
}
