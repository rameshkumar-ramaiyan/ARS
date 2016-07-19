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
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNews.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using ZetaHtmlCompressor;

namespace USDA_ARS.ImportNews
{
   class Program
   {
      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
      static string API_URL_NEWS = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrlForNews");
      static List<News> NEWS_LIST = null;
      static List<ModeCodeLookup> MODE_CODE_LIST = null;
      static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

      static void Main(string[] args)
      {
         AddLog("Getting News From DB...");
         NEWS_LIST = GetNewsAll();

         AddLog("Getting Mode Codes From Umbraco...");
         GenerateModeCodeList(false);
         AddLog("Done. Count: " + MODE_CODE_LIST.Count);
         AddLog("");

         AddLog("Getting New Mode Codes From Umbraco...");
         MODE_CODE_NEW_LIST = GetNewModeCodesAll();

         ImportNews();
      }


      static void ImportNews()
      {
         string htmlPath = ConfigurationManager.AppSettings.Get("NewsArticles:HtmlPath");

         List<NewsTopicItem> newsTopicList = GetTopicItemList();

         List<string> dirs = new List<string>(Directory.EnumerateDirectories(htmlPath));

         foreach (string dirYear in dirs)
         {
            List<string> files = GetNewsHtmlList(dirYear);

            int parentId = 0;
            string year = "";
            int yearInt = 0;

            DirectoryInfo dirInfo = new DirectoryInfo(dirYear);
            year = dirInfo.Name;

            if (int.TryParse(year, out yearInt))
            {
               if (yearInt > 0)
               {
                  parentId = AddNewsYearNode(year);

                  AddLog("========================");
                  AddLog("YEAR ADDED (" + parentId + "): " + year);
               }

               List<ApiContent> apiContentList = new List<ApiContent>();

               foreach (string fileName in files)
               {
                  if (false == fileName.Contains("index.htm") && false == fileName.EndsWith(".jpg"))
                  {
                     FileInfo fileInfo = new FileInfo(fileName);

                     AddLog("Filename: " + fileName);

                     List<string> pathArray = fileInfo.FullName.Split('\\').ToList();

                     AddLog("News Item: " + fileName);

                     string newsTitle = "";
                     string bodyText = GetFileText(fileName);
                     DateTime newsDate = DateTime.MinValue;
                     string newsBlurb = "";

                     List<string> keywordList = null;

                     HtmlDocument doc = new HtmlDocument();
                     doc.LoadHtml(bodyText);

                     News newsItem = NEWS_LIST.Where(p => p.ISFileName.ToLower() == fileInfo.Name.ToLower()).FirstOrDefault();

                     if (newsItem != null)
                     {
                        newsDate = newsItem.DateField;
                     }
                     else if (doc.DocumentNode.SelectSingleNode("//meta[@name='RSSDate']") != null)
                     {
                        DateTime.TryParse(doc.DocumentNode.SelectSingleNode("//meta[@name='RSSDate']").Attributes["content"].Value, out newsDate);
                     }


                     if (newsDate > DateTime.MinValue)
                     {
                        if (doc.DocumentNode.SelectSingleNode("/html/head/title") != null)
                        {
                           newsTitle = doc.DocumentNode.SelectSingleNode("/html/head/title").InnerHtml;

                           string[] newsTitleArray = newsTitle.Split('/');

                           if (newsTitleArray != null && newsTitleArray.Length > 0)
                           {
                              newsTitle = newsTitleArray[0].Trim();

                              newsTitle = newsTitle.Replace("<strong>", "");
                              newsTitle = newsTitle.Replace("</strong>", "");

                              newsTitle = newsTitle.Replace("<STRONG>", "");
                              newsTitle = newsTitle.Replace("</STRONG>", "");
                           }
                        }

                        if (true == string.IsNullOrWhiteSpace(newsTitle))
                        {
                           MatchCollection matches = Regex.Matches(bodyText, "<h2.*?>(.*?)<\\/h2>", RegexOptions.IgnoreCase);
                           if (matches.Count > 0)
                           {
                              newsTitle = matches[0].Groups[1].Value;
                           }
                        }


                        if (doc.DocumentNode.SelectSingleNode("//meta[@name='RSSDescription']") != null)
                        {
                           newsBlurb = doc.DocumentNode.SelectSingleNode("//meta[@name='RSSDescription']").Attributes["content"].Value;
                        }


                        if (doc.DocumentNode.SelectSingleNode("/html/body") != null)
                        {
                           bodyText = doc.DocumentNode.SelectSingleNode("/html/body").InnerHtml;
                        }
                        else
                        {
                           RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                           Regex regx = new Regex("<body[^>]*>(?<theBody>.*)</body>", options);

                           Match match = regx.Match(bodyText);

                           if (match.Success)
                           {
                              bodyText = match.Groups["theBody"].Value;
                           }
                        }

                        if (false == string.IsNullOrEmpty(bodyText))
                        {
                           bodyText = CleanHtml.CleanUpHtml(bodyText);

                           bodyText = ReplaceCaseInsensitive(bodyText, "../thelatest.htm", "/{localLink:8001}");
                           bodyText = ReplaceCaseInsensitive(bodyText, "http://www.ars.usda.gov/is/pr/thelatest.htm", "/{localLink:8001}");

                           bodyText = ReplaceCaseInsensitive(bodyText, "../subscribe.htm", "/{localLink:8002}");
                           bodyText = ReplaceCaseInsensitive(bodyText, "http://www.ars.usda.gov/is/pr/subscribe.htm", "/{localLink:8002}");

                           bodyText = ReplaceCaseInsensitive(bodyText, "../../graphics/", "/ARSUserFiles/news/graphics/");
                           bodyText = ReplaceCaseInsensitive(bodyText, "http://www.ars.usda.gov/is/graphics/", "/ARSUserFiles/news/graphics/");

                           bodyText = ReplaceCaseInsensitive(bodyText, "\"../../", "/");
                           bodyText = ReplaceCaseInsensitive(bodyText, "\"../", "/is/");
                           bodyText = ReplaceCaseInsensitive(bodyText, "http://www.ars.usda.gov/", "/");
                        }

                        if (doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']") != null)
                        {
                           keywordList = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value.Split(',').ToList();
                        }
                        if (keywordList == null)
                        {
                           if (doc.DocumentNode.SelectSingleNode("//meta[@name='RSSKeywords']") != null)
                           {
                              keywordList = doc.DocumentNode.SelectSingleNode("//meta[@name='RSSKeywords']").Attributes["content"].Value.Split(',').ToList();
                           }
                        }

                        bodyText = bodyText.Replace("http://www.ars.usda.gov", "");
                        bodyText = Regex.Replace(bodyText, "\"/is/", "\"/ARSUserFiles/oc/", RegexOptions.IgnoreCase);
                        bodyText = bodyText.Replace("/pandp/people/people.htm?personid=", "/people-locations/person/?person-id=");

                        MatchCollection m1 = Regex.Matches(bodyText, @"/main/site_main\.htm\?modecode=([\d\-]*)", RegexOptions.Singleline);

                        foreach (Match m in m1)
                        {
                           string modeCode = m.Groups[1].Value;

                           ApiResponse responsePage = new ApiResponse();

                           // Get the umbraco page by the mode code (Region/Area or Research Unit)
                           ModeCodeLookup modeCodeLookup = MODE_CODE_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

                           if (modeCodeLookup != null)
                           {
                              bodyText = bodyText.Replace(m.Groups[0].Value, "/{localLink:" + modeCodeLookup.UmbracoId + "}");
                           }
                           else
                           {
                              ModeCodeNew modeCodeNew = MODE_CODE_NEW_LIST.Where(p => p.ModecodeOld == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode)).FirstOrDefault();

                              if (modeCodeNew != null)
                              {
                                 modeCodeLookup = MODE_CODE_LIST.Where(p => p.ModeCode == Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew)).FirstOrDefault();

                                 if (modeCodeLookup != null)
                                 {
                                    bodyText = bodyText.Replace(m.Groups[0].Value, "/{localLink:" + modeCodeLookup.UmbracoId + "}");
                                 }
                              }
                           }
                        }

                        AddLog("Adding News...");
                        ApiContent apiContent = AddNewsArticle(parentId, newsTitle, newsDate, newsBlurb, fileInfo.Name, newsTopicList, keywordList, bodyText, year);

                        apiContentList.Add(apiContent);
                     }
                  }
                  else
                  {
                     AddLog("");
                     AddLog("/ Bypassing file: " + fileName);
                  }
               }

               AddLog("");
               AddLog("Saving News Articles...");
               AddLog("");

               try
               {
                  if (apiContentList != null && apiContentList.Any())
                  {
                     ApiRequest request = new ApiRequest();


                     request.ApiKey = API_KEY;

                     request.ContentList = apiContentList;

                     AddLog("DateStamp-A: " + DateTime.Now);

                     ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                     if (responseBack.ContentList != null)
                     {
                        // Publish the news 
                        if (true)
                        {
                           AddLog("");
                           AddLog("Publishing News: year " + year + "...");

                           ApiRequest requestPublish = new ApiRequest();
                           ApiContent contentPublish = new ApiContent();

                           requestPublish.ApiKey = API_KEY;

                           contentPublish.Id = parentId;

                           requestPublish.ContentList = new List<ApiContent>();
                           requestPublish.ContentList.Add(contentPublish);

                           ApiResponse responseBackPublish = ApiCalls.PostData(requestPublish, "PublishWithChildren");

                           if (responseBackPublish != null)
                           {
                              AddLog(" - Success: " + responseBackPublish.Success);
                              AddLog(" - Message: " + responseBackPublish.Message);
                           }
                        }
                     }
                  }
               }
               catch (Exception ex)
               {
                  AddLog("DateStamp-E: " + DateTime.Now);
                  AddLog("ERROR: " + ex.ToString());
               }


            }
         }

      }


      static List<string> GetNewsHtmlList(string sDir)
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


      static News GetNewsByHtmlName(string htmlName)
      {
         var db = new Database("sitePublisherDbDSN");

         string sql = "SELECT * FROM News WHERE ISFileName = @htmlName";

         News newsItem = db.Query<News>(sql, new { htmlName = htmlName }).FirstOrDefault();

         return newsItem;
      }


      static List<News> GetNewsAll()
      {
         var db = new Database("sitePublisherDbDSN");

         string sql = "SELECT * FROM News WHERE NOT ISFileName IS NULL AND published = 'p'";

         List<News> newsList = db.Query<News>(sql).ToList();

         return newsList;
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



      static List<ModeCodeNew> GetNewModeCodesAll()
      {
         List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT * FROM NewModecodes";

         modeCodeNewList = db.Query<ModeCodeNew>(sql).ToList();

         return modeCodeNewList;
      }


      static int AddNewsYearNode(string year)
      {
         int nodeId = 0;

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         content.Id = 0;
         content.Name = year;
         content.ParentId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("NewsArticles:ParentId"));
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


      static ApiContent AddNewsArticle(int parentId, string title, DateTime newsDate, string blurb, string filename, List<NewsTopicItem> newsTopicList, List<string> keywordList, string htmlText, string year)
      {
         ApiContent content = new ApiContent();

         AddLog(" - News Item: " + title);

         content.Id = 0;
         content.Name = title.Trim();
         content.ParentId = parentId;
         content.DocType = "NewsArticle";
         content.Template = "NewsItem";

         string newsProductItem = ConfigurationManager.AppSettings.Get("NewsArticles:NewProductGuid");

         List<ApiProperty> properties = new List<ApiProperty>();

         properties.Add(new ApiProperty("newsProductsList", newsProductItem));
         properties.Add(new ApiProperty("navigationCategory", "ac79b700-ad67-4179-a4f6-db9705ecce31"));
         properties.Add(new ApiProperty("bodyText", htmlText)); // Body text  

         properties.Add(new ApiProperty("newsBlurb", blurb)); // News Blurb 
         properties.Add(new ApiProperty("oldUrl", ConfigurationManager.AppSettings.Get("NewsArticles:LegacyUrlPrefix") + year + "/" + filename)); // current URL
         properties.Add(new ApiProperty("articleDate", newsDate));

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

                  fieldsetTopic.Alias = "newsTopicSelect";
                  fieldsetTopic.Disabled = false;
                  fieldsetTopic.Id = Guid.NewGuid();
                  fieldsetTopic.Properties = new List<Property>();
                  fieldsetTopic.Properties.Add(new Property("newsTopic", topicGuid));

                  newsTopicArchetypeItem.Fieldsets.Add(fieldsetTopic);
                  // LOOP END
               }
            }

            if (newsTopicArchetypeItem.Fieldsets != null && newsTopicArchetypeItem.Fieldsets.Count > 0)
            {
               string newsTopicsJson = JsonConvert.SerializeObject(newsTopicArchetypeItem, Newtonsoft.Json.Formatting.None, jsonSettings);
               properties.Add(new ApiProperty("newsTopicsList", newsTopicsJson));
            }
         }




         content.Properties = properties;

         content.Save = 1; // 1=Saved, 2=Save And Publish

         return content;
      }

      static string GetFileText(string path)
      {
         return File.ReadAllText(path);
      }


      static string GetTopicGuidByShortName(List<NewsTopicItem> topicList, string shortName)
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


      static List<NewsTopicItem> GetTopicItemList()
      {
         List<NewsTopicItem> newsTopicList = new List<NewsTopicItem>();

         string apiUrl = API_URL_NEWS;

         var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl));
         http.Accept = "application/json";
         http.ContentType = "application/json";
         http.Method = "GET";
         http.Timeout = 216000;

         var httpApiResponse = http.GetResponse();

         var stream = httpApiResponse.GetResponseStream();
         var sr = new StreamReader(stream);
         var httpApiResponseStr = sr.ReadToEnd();

         httpApiResponseStr = httpApiResponseStr.Trim('\"');
         httpApiResponseStr = httpApiResponseStr.Replace("\\", "");

         newsTopicList = JsonConvert.DeserializeObject<List<NewsTopicItem>>(httpApiResponseStr);

         return newsTopicList;
      }


      static string ReplaceCaseInsensitive(string input, string search, string replacement)
      {
         string result = Regex.Replace(
             input,
             Regex.Escape(search),
             replacement.Replace("$", "$$"),
             RegexOptions.IgnoreCase
         );
         return result;
      }


      static void AddLog(string line)
      {
         Debug.WriteLine(line);
         Console.WriteLine(line);
      }

   }

}
