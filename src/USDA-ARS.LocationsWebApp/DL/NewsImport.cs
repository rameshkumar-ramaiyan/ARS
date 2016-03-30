using HtmlAgilityPack;
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
using System.Web;
using Umbraco.Core.Persistence;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class NewsImport
    {
        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        protected static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        public static void ImportNews()
        {
            string htmlPath = ConfigurationManager.AppSettings.Get("NewsArticles:HtmlPath");

            List<NewsTopicItem> newsTopicList = GetTopicItemList();

            List <string> files = GetNewsHtmlList(htmlPath);

            int parentId = 0;
            string year = "";

            foreach (string fileName in files)
            {
                FileInfo fileInfo = new FileInfo(fileName);

                Debug.WriteLine("Filename: " + fileName);

                List<string> pathArray = fileInfo.FullName.Split('\\').ToList();

                if (pathArray != null && pathArray.Count > 0)
                {
                    if (year != pathArray[pathArray.Count - 2])
                    {
                        year = pathArray[pathArray.Count - 2];

                        parentId = AddNewsYearNode(year);

                        Debug.WriteLine("========================");
                        Debug.WriteLine("YEAR ADDED ("+ parentId +"): " + year);
                    }


                    News newsItem = GetNewsByHtmlName(fileInfo.Name);

                    if (newsItem != null)
                    {
                        Debug.WriteLine("News Item: " + fileName);

                        string bodyText = GetFileText(fileName);

                        List<string> keywordList = null;

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(bodyText);

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
                        }

                        if (doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']") != null)
                        {
                            keywordList = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value.Split(',').ToList();
                        }


                        bodyText = bodyText.Replace("http://www.ars.usda.gov", "");
                        bodyText = bodyText.Replace("/pandp/people/people.htm?personid=", "/people-locations/person/?person-id=");

                        MatchCollection m1 = Regex.Matches(bodyText, @"/main/site_main\.htm\?modecode=([\d\-]*)", RegexOptions.Singleline);

                        foreach (Match m in m1)
                        {
                            string modeCode = m.Groups[1].Value;

                            ApiResponse responsePage = new ApiResponse();

                            // Get the umbraco page by the mode code (Region/Area or Research Unit)
                            responsePage = GetCalls.GetNodeByModeCode(modeCode);

                            if (responsePage != null && responsePage.ContentList != null && responsePage.ContentList.Count > 0)
                            {
                                bodyText = bodyText.Replace(m.Groups[0].Value, "/{localLink:"+ responsePage.ContentList[0].Id + "}");
                            }
                        }

                        Debug.WriteLine("Adding News...");
                        AddNewsArticle(parentId, newsItem, newsTopicList, keywordList, bodyText, year);
                    }
                }
            }
        }


        public static List<string> GetNewsHtmlList(string sDir)
        {
            List<String> files = new List<String>();

            foreach (string f in Directory.GetFiles(sDir, "*.htm"))
            {
                files.Add(f);
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                files.AddRange(GetNewsHtmlList(d));
            }


            return files;
        }


        public static News GetNewsByHtmlName(string htmlName)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM News WHERE ISFileName = @htmlName";

            News newsItem = db.Query<News>(sql, new { htmlName = htmlName }).FirstOrDefault();

            return newsItem;
        }


        public static int AddNewsYearNode(string year)
        {
            int nodeId = 0;

            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            content.Id = 0;
            content.Name = year;
            content.ParentId = 1142;
            content.DocType = "NewsFolder";
            content.Template = ""; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();

            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            if (responseBack.ContentList != null)
            {
                nodeId = responseBack.ContentList[0].Id;
            }

            return nodeId;
        }


        public static void AddNewsArticle(int parentId, News newsItem, List<NewsTopicItem> newsTopicList, List<string> keywordList, string htmlText, string year)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            Debug.WriteLine(" - News Item: " + newsItem.SubjectField);

            request.ApiKey = API_KEY;

            content.Id = 0;
            content.Name = newsItem.SubjectField.Trim();
            content.ParentId = parentId;
            content.DocType = "NewsArticle";
            content.Template = "NewsItem";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("newsProductsList", "155a5231-f0d8-4681-810a-1bf1689df114"));
            properties.Add(new ApiProperty("navigationCategory", "ac79b700-ad67-4179-a4f6-db9705ecce31"));
            properties.Add(new ApiProperty("bodyText", htmlText)); // Body text                                                                       
            properties.Add(new ApiProperty("oldUrl", "/is/pr/"+ year + "/" + newsItem.ISFileName)); // current URL
            properties.Add(new ApiProperty("articleDate", newsItem.DateField));

            if (keywordList != null && keywordList.Any())
            {
                // USED FOR ALL ARCHETYPE DATA TYPES
                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                ApiArchetype newsTopicArchetypeItem = new ApiArchetype();

                newsTopicArchetypeItem.Fieldsets = new List<Fieldset>();

                foreach (string keyword in keywordList)
                {
                    string topicGuid = GetTopicGuidByShortName(newsTopicList, keyword);

                    if (false == string.IsNullOrEmpty(topicGuid))
                    {
                        // LOOP START
                        Fieldset fieldsetTopic = new Fieldset();

                        fieldsetTopic.Alias = "newsTopic";
                        fieldsetTopic.Disabled = false;
                        fieldsetTopic.Id = Guid.NewGuid();
                        fieldsetTopic.Properties = new List<Property>();
                        fieldsetTopic.Properties.Add(new Property("value", topicGuid)); // set the file package name

                        newsTopicArchetypeItem.Fieldsets.Add(fieldsetTopic);
                        // LOOP END
                    }
                }

                if (newsTopicArchetypeItem.Fieldsets != null && newsTopicArchetypeItem.Fieldsets.Count > 0)
                {
                    string newsTopicsJson = JsonConvert.SerializeObject(newsTopicArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                    properties.Add(new ApiProperty("newsTopics", newsTopicsJson));
                }
            }




            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            if (responseBack.ContentList != null)
            {

            }
        }

        public static string GetFileText(string path)
        {
            return File.ReadAllText(path);
        }


        public static string GetTopicGuidByShortName(List<NewsTopicItem> topicList, string shortName)
        {
            string output = null;
            NewsTopicItem newsTopicItem = null;

            if (false == string.IsNullOrEmpty(shortName))
            {
                shortName = shortName.ToLower().Trim();

                if (shortName == "animals" || shortName == "cattle" || shortName == "poultry" || shortName == "swine" || shortName == "aquaculture")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Animals").FirstOrDefault();
                }
                else if (shortName == "bees" || shortName == "colony collapse disorder")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Bees").FirstOrDefault();
                }
                else if (shortName == "biofuels")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Biofuels").FirstOrDefault();
                }
                else if (shortName == "biobased products")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Bio Products").FirstOrDefault();
                }
                else if (shortName == "climate change" || shortName == "sustainable agriculture")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Climate Change").FirstOrDefault();
                }
                else if (shortName == "crops" || shortName == "soybeans" || shortName == "corn" || shortName == "wheat" || shortName == "cotton")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Crops").FirstOrDefault();
                }
                else if (shortName == "food safety")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Food Safety").FirstOrDefault();
                }
                else if (shortName == "invasivespecies" || shortName == "invasive species")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Invasive Species").FirstOrDefault();
                }
                else if (shortName == "nutrition" || shortName == "health")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Nutrition and Health").FirstOrDefault();
                }
                else if (shortName == "organics" || shortName == "organics")
                {
                    newsTopicItem = topicList.Where(p => p.Text == "Organics").FirstOrDefault();
                }

                if (newsTopicItem != null)
                {
                    output = newsTopicItem.Value;
                }
            }

            return output;
        }


        public static List<NewsTopicItem> GetTopicItemList()
        {
            List<NewsTopicItem> newsTopicList = new List<NewsTopicItem>();

            string apiUrl = API_URL;

            var http = (HttpWebRequest)WebRequest.Create(new Uri("http://technik-usda.imulus.io/umbraco/usda/NewsTopicPicker/Get"));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "GET";
            http.Timeout = 216000;

            var httpApiResponse = http.GetResponse();

            var stream = httpApiResponse.GetResponseStream();
            var sr = new StreamReader(stream);
            var httpApiResponseStr = sr.ReadToEnd();

            httpApiResponseStr = httpApiResponseStr.Trim('\"');
            httpApiResponseStr = httpApiResponseStr.Replace("\\","");

            newsTopicList = JsonConvert.DeserializeObject<List<NewsTopicItem>>(httpApiResponseStr);

            return newsTopicList;
        }

    }
}