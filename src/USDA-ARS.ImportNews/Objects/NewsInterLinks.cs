using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNews.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportNews.Objects
{
   public class NewsInterLinks
   {
      public static List<NewsInterLink> GenerateInterLinks(int umbracoNodeId, Guid umbracoNodeGuid, List<LinkItem> linkList, List<ModeCodeLookup> modeCodeList, List<ModeCodeNew> newModeCodeList = null)
      {
         List<NewsInterLink> interLinkList = new List<NewsInterLink>();

         if (linkList != null && linkList.Any())
         {
            foreach (LinkItem linkItem in linkList)
            {
               NewsInterLink interLinkItem = new NewsInterLink();

               interLinkItem.Id = Guid.Empty;

               interLinkItem.UmbracoNodeId = umbracoNodeId;
               interLinkItem.UmbracoNodeGuid = umbracoNodeGuid;

               if (linkItem.Href != null)
               {
                  if (linkItem.Href.IndexOf("?person-id") >= 0)
                  {
                     int personId = 0;

                     Match m2 = Regex.Match(linkItem.Href, @"\?person\-id\=(.*)", RegexOptions.Singleline);
                     if (m2.Success)
                     {
                        int.TryParse(m2.Groups[1].Value, out personId);
                     }

                     if (personId > 0)
                     {
                        interLinkItem.LinkType = "person";
                        interLinkItem.LinkId = personId;
                     }
                  }
                  else if (linkItem.Href.IndexOf("/main/site_main.htm?modecode=") >= 0)
                  {
                     string modeCode = linkItem.Href.ToLower().Replace("/main/site_main.htm?modecode=", "");
                     ModeCodeLookup modeCodeLookup = null;

                     modeCodeLookup = modeCodeList.Where(p => p.ModeCode == ModeCodes.ModeCodeAddDashes(modeCode)).FirstOrDefault();

                     if (modeCodeLookup != null)
                     {
                        modeCode = modeCodeLookup.ModeCode;
                     }
                     else if (newModeCodeList != null)
                     {
                        ModeCodeNew modeCodeNew = newModeCodeList.Where(p => p.ModecodeOld == ModeCodes.ModeCodeNoDashes(modeCode)).FirstOrDefault();

                        if (modeCodeNew != null)
                        {
                           modeCodeLookup = modeCodeList.Where(p => p.ModeCode == ModeCodes.ModeCodeAddDashes(modeCodeNew.ModecodeNew)).FirstOrDefault();

                           if (modeCodeLookup != null)
                           {
                              modeCode = modeCodeLookup.ModeCode;
                           }
                        }
                     }

                     if (false == string.IsNullOrEmpty(modeCode))
                     {
                        modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

                        modeCode = DoesNodeHaveSingleResearchUnits(modeCode, newModeCodeList);

                        interLinkItem.LinkType = "place";
                        interLinkItem.LinkId = Convert.ToInt64(modeCode);
                     }
                  }
                  else if (linkItem.Href.IndexOf("/{localLink:") >= 0)
                  {
                     //{localLink:2188}
                     int nodeId = 0;

                     Match m2 = Regex.Match(linkItem.Href, @"{localLink:([\d]*)}", RegexOptions.Singleline);
                     if (m2.Success)
                     {
                        int.TryParse(m2.Groups[1].Value, out nodeId);
                     }

                     if (nodeId > 0)
                     {
                        string modeCode = "";
                        ModeCodeLookup modeCodeLookup = null;

                        modeCodeLookup = modeCodeList.Where(p => p.UmbracoId == nodeId).FirstOrDefault();

                        if (modeCodeLookup != null)
                        {
                           modeCode = modeCodeLookup.ModeCode;
                        }

                        if (false == string.IsNullOrEmpty(modeCode))
                        {
                           modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

                           modeCode = DoesNodeHaveSingleResearchUnits(modeCode, newModeCodeList);

                           interLinkItem.LinkType = "place";
                           interLinkItem.LinkId = Convert.ToInt64(modeCode);
                        }
                     }
                  }
                  else
                  {
                     string url = linkItem.Href.Replace("http://www.ars.usda.gov", "");

                     ModeCodeLookup node = modeCodeList.Where(p => p.Url.ToLower() == url.ToLower()).FirstOrDefault();

                     if (node != null)
                     {
                        string modeCode = node.ModeCode;

                        if (false == string.IsNullOrEmpty(modeCode))
                        {
                           modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

                           modeCode = DoesNodeHaveSingleResearchUnits(modeCode, newModeCodeList);

                           interLinkItem.LinkType = "place";
                           interLinkItem.LinkId = Convert.ToInt64(modeCode);
                        }
                     }
                  }
               }

               if (false == string.IsNullOrEmpty(interLinkItem.LinkType))
               {
                  AddLink(interLinkItem);

                  interLinkList.Add(interLinkItem);
               }
            }
         }

         return interLinkList;
      }


      public static List<LinkItem> FindInterLinks(string text)
      {
         List<LinkItem> list = new List<LinkItem>();

         // 1.
         // Find all matches in file.
         MatchCollection m1 = Regex.Matches(text, @"(<a.*?>.*?</a>)",
             RegexOptions.Singleline);

         // 2.
         // Loop over each match.
         foreach (Match m in m1)
         {
            string value = m.Groups[1].Value;
            LinkItem i = new LinkItem();

            // 3.
            // Get href attribute.
            Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
            RegexOptions.Singleline);
            if (m2.Success)
            {
               i.Href = m2.Groups[1].Value;
            }

            // 4.
            // Remove inner tags from text.
            string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
            RegexOptions.Singleline);
            i.Text = t;

            list.Add(i);
         }
         return list;
      }


      public static NewsInterLink AddLink(NewsInterLink linkItem)
      {
         var db = new Database("arisPublicWebDbDSN");

         if (linkItem != null)
         {
            if (linkItem.Id == Guid.Empty)
            {
               linkItem.Id = Guid.NewGuid();

               db.Insert(linkItem);
            }
            else
            {
               db.Update(linkItem);
            }
         }

         return linkItem;
      }


      public static ApiContent GetUmbracoPageByModeCode(string foundModeCode)
      {
         ApiResponse modeCodeNode = GetCalls.GetNodeByModeCode(foundModeCode);
         ApiContent node = null;

         if (modeCodeNode != null && modeCodeNode.ContentList != null && modeCodeNode.ContentList.Any() && true == modeCodeNode.ContentList[0].Success)
         {
            node = modeCodeNode.ContentList[0];
         }

         return node;
      }


      public static string DoesNodeHaveSingleResearchUnits(string foundModeCode, List<ModeCodeNew> newModeCodeList)
      {
         string modeCode = foundModeCode;

         try
         {
            foundModeCode = ModeCodes.ModeCodeNoDashes(foundModeCode);

            // Check for old Mode Code
            ModeCodeNew newModeCode = newModeCodeList.Where(p => p.ModecodeOld == foundModeCode).FirstOrDefault();

            if (newModeCode != null)
            {
               foundModeCode = newModeCode.ModecodeOld;
               modeCode = foundModeCode;
            }

            string sql = "exec [uspgetAllReassignModeCodesForCityWithSingleChild] " + foundModeCode;
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["arisPublicWebDbDSN"].ConnectionString);

            try
            {
               SqlDataAdapter da = new SqlDataAdapter();
               SqlCommand sqlComm = new SqlCommand(sql, conn);

               da.SelectCommand = sqlComm;
               da.SelectCommand.CommandType = CommandType.Text;

               DataSet ds = new DataSet();
               da.Fill(ds, "ModeCode");

               dt = ds.Tables["ModeCode"];

               if (dt != null && dt.Rows.Count > 0)
               {
                  if (dt.Rows[0][0] != null && false == string.IsNullOrEmpty(dt.Rows[0].Field<string>(0)))
                  {
                     modeCode = dt.Rows[0].Field<string>(0);
                  }
               }
            }
            catch (Exception ex)
            {
               throw ex;
            }
            finally
            {
               conn.Close();
            }
         }
         catch(Exception ex)
         {
            //
         }

         //ApiContent node = GetUmbracoPageByModeCode(foundModeCode);

         //if (node != null)
         //{
         //   ApiProperty modeCodeFoundMain = node.Properties.Where(p => p.Key == "modeCode").FirstOrDefault();
         //   if (modeCodeFoundMain != null)
         //   {
         //      modeCode = modeCodeFoundMain.Value.ToString();
         //   }
         //}

         //if (node != null && node.ChildContentList != null && node.ChildContentList.Any())
         //{
         //   List<ApiContent> researchUnitList = node.ChildContentList.Where(p => p.DocType == "ResearchUnit").ToList();

         //   if (researchUnitList != null && researchUnitList.Count == 1)
         //   {
         //      ApiProperty modeCodeFound = researchUnitList[0].Properties.Where(p => p.Key == "modeCode").FirstOrDefault();

         //      if (modeCodeFound != null)
         //      {
         //         modeCode = modeCodeFound.Value.ToString();
         //      }
         //   }
         //}

         return modeCode;
      }
   }
}
