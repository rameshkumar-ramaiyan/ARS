using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportLocationMap.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportLocationMap
{
    class Program
    {
        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static int UMBRACO_ID = 7998;

        static void Main(string[] args)
        {
            List<MapCoordinate> mapCoordinateList = null;

            AddLog("Getting Mode Codes From Umbraco...");
            MODE_CODE_LIST = GetModeCodesAll();

            var db = new Database("sitePublisherDbDSN");

            string sql = @"select m.modecodeconc,  m.modecode_1, m.modecode_2, m.modecode_3, m.modecode_4,
			                    m.modecode_1_desc,
			                    m.modecode_2_desc, m.modecode_3_desc,
			                    a.coordinates,
			                    m.state_code stateabbr
	                     from

		                    aris_public_web.dbo.v_locations m, mapcoordinates a

	                    where m.modecode_1 = a.mode_1
	                    and  m.modecode_2 = a.mode_2
	                    and m.modecode_3 = a.mode_3
	                    and  m.modecode_4 = '00'
	                    and status_code = 'a'";

            mapCoordinateList = db.Query<MapCoordinate>(sql).ToList();


            //{"fieldsets":[{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21362\",\r\n    \"name\": \"Fort Collins, Colorado\",\r\n    \"url\": \"/plains-area/fort-collins-co/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"191,159"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"7555c2b9-7c91-44d4-b812-1894ede7f277"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21299\",\r\n    \"name\": \"Plains Area\",\r\n    \"url\": \"/plains-area/\",\r\n    \"icon\": \"icon-map-location color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"191,159"},{"alias":"isARegion","value":1}],"alias":"mapCoordinates","disabled":false,"id":"93482c4f-82f8-411f-b8a8-e951feb86dbb"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21371\",\r\n    \"name\": \"Las Cruces, New Mexico\",\r\n    \"url\": \"/plains-area/las-cruces-nm/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"148,250"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"9a6652ed-d055-4470-ba74-3e9bb489f443"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21341\",\r\n    \"name\": \"Maricopa, Arizona\",\r\n    \"url\": \"/pacific-west-area/maricopa-az/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"100,228"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"00e54b23-1502-4b97-8303-0174e20e71a9"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21342\",\r\n    \"name\": \"Tucson, Arizona\",\r\n    \"url\": \"/pacific-west-area/tucson-az/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"108,242"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"4fcfe6c1-4966-446b-ae9b-ba026b978952"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21357\",\r\n    \"name\": \"Logan, Utah\",\r\n    \"url\": \"/pacific-west-area/logan-ut/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"117,132"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"9cb7b2f4-e70a-40b6-9bb1-3ea3e01175fe"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"1075\",\r\n    \"name\": \"ARS Home\",\r\n    \"url\": \"/\",\r\n    \"icon\": \"icon-home\"\r\n  }\r\n]"},{"alias":"coordinates","value":"492,199"},{"alias":"isARegion","value":1}],"alias":"mapCoordinates","disabled":false,"id":"a595dde7-9a7d-4494-b41e-b909ff98374e"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21331\",\r\n    \"name\": \"Beltsville, Maryland (Barc)\",\r\n    \"url\": \"/northeast-area/beltsville-md/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"442,152"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"a43f5559-ed80-4dfa-9732-ae0cd539e321"},{"properties":[{"alias":"site","value":"[\r\n  {\r\n    \"id\": \"21353\",\r\n    \"name\": \"Reno, Nevada\",\r\n    \"url\": \"/pacific-west-area/reno-nv/\",\r\n    \"icon\": \"icon-company color-green\"\r\n  }\r\n]"},{"alias":"coordinates","value":"42,141"},{"alias":"isARegion","value":0}],"alias":"mapCoordinates","disabled":false,"id":"eb16651c-cafa-41c0-8de1-fc21d2c42ce7"}]}


            ApiResponse apiResponse = GetCalls.GetNodeByUmbracoId(UMBRACO_ID);

            if (apiResponse != null && apiResponse.ContentList != null && apiResponse.ContentList.Count == 1)
            {
                ApiContent content = apiResponse.ContentList[0];

                if (content != null)
                {
                    List<ApiProperty> properties = new List<ApiProperty>();

                    // USED FOR ALL ARCHETYPE DATA TYPES
                    var jsonSettings = new JsonSerializerSettings();
                    jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                    ApiArchetype mapArchetypeItem = new ApiArchetype();

                    mapArchetypeItem.Fieldsets = new List<Fieldset>();


                    foreach (MapCoordinate mapCoord in mapCoordinateList)
                    {
                        ModeCodeLookup modeCodeFound = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(mapCoord.ModeCodeConc)).FirstOrDefault();

                        if (modeCodeFound != null)
                        {
                            // LOOP START
                            Fieldset fieldsetTopic = new Fieldset();

                            fieldsetTopic.Alias = "mapCoordinates";
                            fieldsetTopic.Disabled = false;
                            fieldsetTopic.Id = Guid.NewGuid();

                            fieldsetTopic.Properties = new List<Property>();

                            string siteName = mapCoord.ModeCode1Desc;

                            if (false == string.IsNullOrEmpty(mapCoord.ModeCode2Desc))
                            {
                                siteName += " - " + mapCoord.ModeCode2Desc;
                            }

                            string icon = "icon-map-location color-green";

                            if (false == string.IsNullOrEmpty(mapCoord.ModeCode2Desc))
                            {
                                icon = "icon-company color-green";
                            }

                            Site siteMap = new Site(modeCodeFound.UmbracoId.ToString(), siteName, modeCodeFound.Url, icon); // set the url path
                            fieldsetTopic.Properties.Add(new Property("site", "[" + JsonConvert.SerializeObject(siteMap, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));

                            fieldsetTopic.Properties.Add(new Property("coordinates", mapCoord.Coordinates));
                            if (mapCoord.ModeCode2 == "00")
                            {
                                fieldsetTopic.Properties.Add(new Property("isARegion", "1"));
                            }
                            else
                            {
                                fieldsetTopic.Properties.Add(new Property("isARegion", "0"));
                            }

                            mapArchetypeItem.Fieldsets.Add(fieldsetTopic);
                            // LOOP END
                        }
                    }

                    if (mapArchetypeItem.Fieldsets != null && mapArchetypeItem.Fieldsets.Count > 0)
                    {
                        string mapJson = JsonConvert.SerializeObject(mapArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                        properties.Add(new ApiProperty("mapCoordinates", mapJson));
                    }



                    content.Properties = properties;

                    content.Save = 2; // 1=Saved, 2=Save And Publish





                    ApiRequest request = new ApiRequest();

                    AddLog("Saving News Articles...");

                    request.ApiKey = API_KEY;

                    request.ContentList = new List<ApiContent>();
                    request.ContentList.Add(content);

                    ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                    if (responseBack.ContentList != null)
                    {

                    }
                }
            }

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

        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
        }
    }
}
