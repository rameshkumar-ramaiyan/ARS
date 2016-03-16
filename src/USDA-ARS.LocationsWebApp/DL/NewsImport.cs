using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Persistence;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class NewsImport
    {
        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        public static void ImportNews()
        {
            List<string> files = GetNewsHtmlList();

            foreach (string fileName in files)
            {
                FileInfo fileInfo = new FileInfo(fileName);

                News newsItem = GetNewsByHtmlName(fileInfo.Name);

                if (newsItem != null)
                {
                    string bodyText = GetFileText(fileName);

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

                    bodyText = bodyText.Replace("http://www.ars.usda.gov", "");
                    bodyText = bodyText.Replace("/pandp/people/people.htm?personid=", "/people-locations/person/?person-id=");

                    MatchCollection m1 = Regex.Matches(bodyText, @"/main/site_main\.htm\?modecode=([\d\-]*)", RegexOptions.Singleline);

                    foreach (Match m in m1)
                    {
                        string modeCode = m.Groups[1].Value;

                        ApiResponse responsePage = new ApiResponse();

                        // Get the umbraco page by the mode code (Region/Area or Research Unit)
                        responsePage = GetCalls.GetNodeByModeCode(modeCode);

                        if (responsePage != null && responsePage.ContentList != null)
                        {
                            bodyText = bodyText.Replace(m.Groups[0].Value, responsePage.ContentList[0].Url);
                        }
                    }

                    AddNewsArticle(newsItem, bodyText);
                }
            }
        }


        public static List<string> GetNewsHtmlList()
        {
            string htmlPath = @"C:\_Repo\USDA-ARS.Umbraco\src\2015";

            List<string> files = Directory.GetFiles(htmlPath, "*.htm").ToList();

            return files;
        }


        public static News GetNewsByHtmlName(string htmlName)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM News WHERE ISFileName = @htmlName";

            News newsItem = db.Query<News>(sql, new { htmlName = htmlName }).FirstOrDefault();

            return newsItem;
        }


        public static void AddNewsArticle(News newsItem, string htmlText)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            content.Id = 0;
            content.Name = newsItem.SubjectField.Trim();
            content.ParentId = 9173;
            content.DocType = "NewsArticle";
            content.Template = "NewsItem"; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", htmlText)); // Body text                                                                       
            properties.Add(new ApiProperty("oldUrl", "/is/pr/2015/" + newsItem.ISFileName)); // current URL
            properties.Add(new ApiProperty("articleDate", newsItem.DateField));

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
    }
}