using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using System.Net;
using System.Data.SqlClient;

namespace USDA_ARS.ImportNP
{
   class Program
   {
      static string LocationConnectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
      static int UMBRACO_NODE_ID_NATIONAL_PROGRAM_MAIN = 31699;
      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

      static string LOG_FILE_TEXT = "";

      static DateTime TIME_STARTED = DateTime.MinValue;
      static DateTime TIME_ENDED = DateTime.MinValue;


      static void Main(string[] args)
      {
         AddLog("-= ADD NATIONAL PROGRAMS =-");
         AddLog("");
         AddLog("");
         AddLog("Getting National Programs into Umbraco...");

         try
         {
            TIME_STARTED = DateTime.Now;

            ImportNationalPrograms();

            AddLog("");
            AddLog("");
            AddLog("");
            AddLog("/// IMPORT COMPLETE ///");
            AddLog("");

            TIME_ENDED = DateTime.Now;

            TimeSpan timeLength = TIME_ENDED.Subtract(TIME_STARTED);

            AddLog("/// Time to complete: " + timeLength.ToString(@"hh") + " hours : " + timeLength.ToString(@"mm") + " minutes : " + timeLength.ToString(@"ss") + " seconds ///");
         }
         catch (Exception ex)
         {
            AddLog("!!!! ERROR !!!!");
            AddLog(ex.ToString());
         }

         using (FileStream fs = File.Create("NP_LOG_FILE.txt"))
         {
            // Add some text to file
            Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
            fs.Write(fileText, 0, fileText.Length);
         }
      }


      static void ImportNationalPrograms()
      {
         // GET NATIONAL PROGRAM GROUPS
         List<NationalProgramGroup> nationalProgramGroupList = CreateNationalProgramGroups();

         foreach (NationalProgramGroup npGroup in nationalProgramGroupList)
         {
            // You can now get the National Program Items by either:
            // Mode Code: npGroup.ModeCode
            // or...
            // NPGroupId: npGroup.NPGroupId

            DataTable legacyNPProgramItems = GetAllNationalProgramItems(npGroup.NPGroupId); // The Mode Code must be in xx-xx-xx-xx format

            AddLog("");

            for (int legacyNPProgramsRowId = 0; legacyNPProgramsRowId < legacyNPProgramItems.Rows.Count; legacyNPProgramsRowId++)
            {
               // ========================
               // GET THE INFORMATION FOR THE NATIONAL PROGRAM ITEM
               // ========================

               AddLog("Grab National Program Items...");

               //string npTitle = "NATIONAL PROGRAM TITLE";
               //string npCode = "NP_CODE_HERE";
               //string npText = "BODY TEXT OF NATION PROGRAM ITEM";
               string npTitle = legacyNPProgramItems.Rows[legacyNPProgramsRowId].Field<string>(2);
               string npCode = legacyNPProgramItems.Rows[legacyNPProgramsRowId].Field<int>(1).ToString();
               DataTable legacyNPProgramDocuments = GetAllNPDocumentsStrategicVersionOnNPNumber(npCode);
               string npText = legacyNPProgramDocuments.Rows[0].Field<string>(0);

               ApiContent content = new ApiContent();

               AddLog("");

               AddLog("NP: (" + npCode + ")" + npTitle);

               content = GenerateNationalProgramItem(npGroup.UmbracoId, npTitle.Trim(), npCode.Trim(), npText);
               // Get the Documents by National Program.
               DataTable legacyNPProgramDocs = GetAllNationalProgramDocuments(npCode);
               ApiRequest request = new ApiRequest();

               request.ApiKey = API_KEY;

               request.ContentList = new List<ApiContent>();
               request.ContentList.Add(content);

               AddLog(" - Saving...");

               ApiResponse responseBack = ApiCalls.PostData(request, "Post");

               if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
               {
                  int umbracoParentId = responseBack.ContentList[0].Id;

                  AddLog(" - Saved: Umbraco ID: " + umbracoParentId);

                  ApiResponse responseDocsFolder = new ApiResponse();

                  AddLog(" - Getting DOCS Folder...");

                  // Get DOCS Folder: The umbraco page by the NP Code
                  responseDocsFolder = GetCalls.GetNodeByNationalProgramCode(npCode);

                  if (responseDocsFolder != null && true == responseDocsFolder.Success && responseDocsFolder.ContentList != null && responseDocsFolder.ContentList.Count > 0)
                  {
                     ApiContent docsFolder = responseDocsFolder.ContentList[0].ChildContentList.Where(p => p.DocType == "NationalProgramFolderContainer").FirstOrDefault();

                     if (docsFolder != null)
                     {
                        AddLog(" - DOCS Folder found: Umbraco Id: " + docsFolder.Id);

                        // We now have the Docs Folder for the National Program Item. Lets get the docs from sitepublisher...

                        List<ApiContent> contentDocPages = new List<ApiContent>(); // Create the ApiContent List for all the docs for a NP item.

                        //// Get the Documents by National Program.
                        //DataTable legacyNPProgramDocs = GetAllNationalProgramDocuments(npCode); // SHOULD YOU GET BY NP CODE INSTEAD?

                        AddLog(" - Getting Documents for NP...");
                        AddLog(" - Documents Count: " + legacyNPProgramDocs.Rows.Count);
                        AddLog("");

                        for (int legacyNPProgramsDocRowId = 0; legacyNPProgramsDocRowId < legacyNPProgramDocs.Rows.Count; legacyNPProgramsDocRowId++)
                        {
                           // ========================
                           // GET THE INFORMATION FOR THE NATIONAL PROGRAM ITEM DOCUMENTS
                           // ========================

                           //string legacyDocType = "SITEPUBLISHER DOC TYPE";// Get the Legacy sitepublisher doctype. THIS NEEDS TO BE: Program Inputs, Program Planning, or Program Reports
                           //string legacyDocTitle = "DOC TITLE HERE";
                           //string legacyDocText = "DOC TEXT HERE";
                           //string oldDocId = "OLD SITEPUBLSHER DOC ID";

                           string legacyDocType = legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<string>(3).Trim();// Get the Legacy sitepublisher doctype. THIS NEEDS TO BE: Program Inputs, Program Planning, or Program Reports
                           string legacyDocTitle = legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<string>(1).Trim();
                           string legacyDocText = "";

                           AddLog(" - Document: " + legacyDocTitle);

                           if (!string.IsNullOrEmpty(legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<string>(6)))
                           {
                              string legacyDocTextEncrypted = legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<string>(6);
                              DataTable dtEncryptedlegacyNPProgramDocs = new DataTable(legacyDocTextEncrypted);
                              dtEncryptedlegacyNPProgramDocs = GetAllNPDocPagesDecrypted(legacyDocTextEncrypted);
                              legacyDocText = dtEncryptedlegacyNPProgramDocs.Rows[0].Field<string>(0);
                              //legacyDocText = legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<string>(6).Trim(); // TODO: GET THE PAGE TEXT AND ADD IT HERE
                              legacyDocText = StripHTML(legacyDocText);
                              legacyDocText = StripHTMLAdditional(legacyDocText);
                              legacyDocText = CleanHtml.CleanUpHtml(legacyDocText);
                           }
                           string oldDocId = legacyNPProgramDocs.Rows[legacyNPProgramsDocRowId].Field<int>(0).ToString();

                           string umbracoDocType = "";

                           if (legacyDocType == "Program Inputs")
                           {
                              umbracoDocType = "NPProgramInputs";
                           }
                           else if (legacyDocType == "Program Planning")
                           {
                              umbracoDocType = "NPProgramPlanning";
                           }
                           else if (legacyDocType == "Program Reports")
                           {
                              umbracoDocType = "NPProgramReports";
                           }
                           else
                           {
                              throw new Exception("NEED A DOC TYPE FOR UMBRACO!");
                           }

                           AddLog(" - Prep for saving...");

                           // Add the doc page to the ApiContent
                           ApiContent docPage = GenerateNationalProgramDocument(docsFolder.Id, legacyDocTitle, umbracoDocType, legacyDocText, npCode.ToString(), oldDocId);

                           if (docPage != null)
                           {
                              // Add the doc page to the ApiContent Collection List
                              contentDocPages.Add(docPage);
                           }
                        }

                        AddLog("");

                        AddLog(" - Saving all documents...");

                        // POST all the docs for a national program item
                        ApiRequest requestDocs = new ApiRequest();

                        requestDocs.ApiKey = API_KEY;

                        requestDocs.ContentList = new List<ApiContent>();
                        requestDocs.ContentList = contentDocPages;

                        ApiResponse responseBackDocs = ApiCalls.PostData(requestDocs, "Post");

                        if (responseBackDocs.ContentList != null && responseBackDocs.ContentList.Count > 0)
                        {
                           AddLog(" - Saved.");
                        }
                        else
                        {
                           AddLog(" - !!! ERROR !!!");
                        }

                     }
                  }
               }


               AddLog("");
               AddLog("");
            }
         }
      }

      static string DeleteAllNationalProgramItems()
      {
         string output = "";

         // GET NATIONAL PROGRAM GROUPS
         List<NationalProgramGroup> nationalProgramGroupList = GetNationalProgramGroups();

         foreach (NationalProgramGroup npGroup in nationalProgramGroupList)
         {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            content.Id = npGroup.UmbracoId; // National Program Group

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = ApiCalls.PostData(request, "Get");

            if (responseBack != null)
            {
               if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
               {
                  ApiContent reqContent = responseBack.ContentList[0];

                  if (reqContent.ChildContentList != null && reqContent.ChildContentList.Count > 0)
                  {
                     ApiResponse responseDeleteBack = DeleteNationalProgramNodes(reqContent.ChildContentList);

                     output += "<hr />\r\n";
                     output += "<strong>Delete</strong><br />\r\n";
                     output += "<hr />\r\n";

                     if (responseDeleteBack != null)
                     {
                        output += "Success: " + responseBack.Success + "<br />\r\n";
                        output += "Message: " + responseBack.Message + "<br />\r\n";
                        output += "<br />\r\n";

                        if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                        {
                           foreach (ApiContent child in responseBack.ContentList)
                           {
                              output += "Success: " + child.Success + "<br />\r\n";
                              output += "Content Id: " + child.Id + "<br />\r\n";
                              output += "Content Name: " + child.Name + "<br />\r\n";
                              output += "<br />\r\n";
                           }

                        }
                     }
                  }
               }
            }
         }

         return output;
      }

      static ApiResponse DeleteNationalProgramNodes(List<ApiContent> contentList)
      {
         ApiResponse response = null;
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;
         request.ContentList = contentList;

         response = ApiCalls.PostData(request, "Delete");

         return response;
      }

      //static DataTable GetAllNationalProgramGroups()
      //{
      //    Locations locationsResponse = new Locations();
      //    string sql = "[uspgetAllNPGroups]";
      //    DataTable dt = new DataTable();
      //    SqlConnection conn = new SqlConnection(LocationConnectionString);

      //    try
      //    {
      //        SqlDataAdapter da = new SqlDataAdapter();
      //        SqlCommand sqlComm = new SqlCommand(sql, conn);


      //        da.SelectCommand = sqlComm;
      //        da.SelectCommand.CommandType = CommandType.StoredProcedure;
      //        //sqlComm.Parameters.AddWithValue("@ModeCode", modeCode);

      //        DataSet ds = new DataSet();
      //        da.Fill(ds, "Locations");

      //        dt = ds.Tables["Locations"];
      //        //foreach (DataRow dr in dt.Rows)
      //        //{
      //        //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
      //        //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



      //        //}



      //    }
      //    catch (Exception ex)
      //    {
      //        throw ex;
      //    }
      //    finally
      //    {
      //        conn.Close();
      //    }

      //    //return locationsResponse;
      //    return dt;
      //}
      static DataTable GetAllNationalProgramItems(int groupId)
      {
         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllNPGroupItemsBasedOnGroupId]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@GroupId", groupId);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }
      static DataTable GetAllNPDocumentsStrategicVersionOnNPNumber(string nPNumber)
      {
         // YOU WILL NEED TO GET THE DOCUMENTS BY THE NP CODE

         //SELECT title, rtrim(doctype) as doctype, docid
         //FROM documents d
         //WHERE d.originsite_type = 'program'
         //AND d.originsite_id = @npCode
         //AND d.published = 'p'
         //AND d.SPSysEndTime is null
         //ORDER BY rtrim(doctype), title, docid

         // THE ABOVE SQL STATEMENT WILL GET YOU THE DOC TITLE, DOC TYPE, AND DOC ID






         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllNPDocumentsStrategicVersionOnNPNumber]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@NPNumber", nPNumber);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }
      static DataTable GetAllNationalProgramDocuments(string nPNumber)
      {
         // YOU WILL NEED TO GET THE DOCUMENTS BY THE NP CODE

         //SELECT title, rtrim(doctype) as doctype, docid
         //FROM documents d
         //WHERE d.originsite_type = 'program'
         //AND d.originsite_id = @npCode
         //AND d.published = 'p'
         //AND d.SPSysEndTime is null
         //ORDER BY rtrim(doctype), title, docid

         // THE ABOVE SQL STATEMENT WILL GET YOU THE DOC TITLE, DOC TYPE, AND DOC ID






         Locations locationsResponse = new Locations();
         string sql = "[uspgetAllNPDocumentsBasedOnNPNumber]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@NPNumber", nPNumber);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }
      static DataTable GetAllNPDocPagesDecrypted(string docPageEncrypted)
      {





         Locations locationsResponse = new Locations();
         string sql = "[uspGetAllNPDocPagesDecrypted]";
         DataTable dt = new DataTable();
         SqlConnection conn = new SqlConnection(LocationConnectionString);

         try
         {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand sqlComm = new SqlCommand(sql, conn);


            da.SelectCommand = sqlComm;
            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlComm.Parameters.AddWithValue("@DocPageEncrypted", docPageEncrypted);

            DataSet ds = new DataSet();
            da.Fill(ds, "Locations");

            dt = ds.Tables["Locations"];
            //foreach (DataRow dr in dt.Rows)
            //{
            //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
            //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



            //}



         }
         catch (Exception ex)
         {
            throw ex;
         }
         finally
         {
            conn.Close();
         }

         //return locationsResponse;
         return dt;
      }

      //inputs-group id , name, , and no mode code
      //out puts-content id
      //static ApiContent GenerateNationalProgramGroup(string name, string modeCode)
      //{
      //    ApiContent content = new ApiContent();

      //    content.Id = 0;
      //    content.Name = name;
      //    content.ParentId = 1126; // Programs node
      //    content.DocType = "NationalProgramGroup";
      //    content.Template = "NationalProgramGroup";

      //    List<ApiProperty> properties = new List<ApiProperty>();

      //    properties.Add(new ApiProperty("modeCode", modeCode)); // Mode Code
      //    properties.Add(new ApiProperty("oldUrl", "/pandp/locations/NPSLocation.htm?modecode=" + modeCode)); // current URL               

      //    content.Properties = properties;

      //    content.Save = 2;

      //    return content;
      //}
      //inputs-np code(mode code) , name,body text old url, parent umbraco id
      //out puts-content id

      static ApiContent GenerateNationalProgramItem(int parentId, string name, string npCode, string bodyText)
      {
         ApiContent content = new ApiContent();

         content.Id = 0;
         content.Name = name;
         content.ParentId = parentId;
         content.DocType = "NationalProgram";
         content.Template = "NationalProgramPage";

         List<ApiProperty> properties = new List<ApiProperty>();

         properties.Add(new ApiProperty("bodyText", bodyText)); // Body Text
         properties.Add(new ApiProperty("npCode", npCode)); // NP Code (National Program Code)
         properties.Add(new ApiProperty("folderLabel", "np" + npCode)); // NP Code (National Program Code)
         properties.Add(new ApiProperty("navigationCategory", ""));
         //properties.Add(new ApiProperty("navigationCategoryBottom", "0ad2420a-b81b-4f59-be86-e9e1b298f09c"));
         properties.Add(new ApiProperty("oldUrl", "/research/programs/programs.htm?NP_CODE=" + npCode)); // current URL 

         content.Properties = properties;

         content.Save = 2;

         return content;
      }

      //inputs-np code(mode code) , name,body text old url,doc type , parent umbraco id( is this np or npgroup id?)
      //out puts-content id

      static ApiContent GenerateNationalProgramDocument(int parentId, string name, string docType, string bodyText, string npCode, string oldDocId)
      {
         ApiContent content = new ApiContent();

         content.Id = 0;
         content.Name = name;
         content.ParentId = parentId;
         content.DocType = docType;
         content.Template = "NationalProgramDocument";

         List<ApiProperty> properties = new List<ApiProperty>();

         bodyText = CleanHtml.CleanUpHtml(bodyText);

         properties.Add(new ApiProperty("bodyText", bodyText)); // Body Text
         properties.Add(new ApiProperty("oldUrl", "/research/programs/programs.htm?np_code=" + npCode + "&docid=" + oldDocId)); // current URL   
         properties.Add(new ApiProperty("oldId", oldDocId)); // current URL   

         content.Properties = properties;

         content.Save = 2;

         return content;
      }


      static int GenerateNationalProgramGroup(string name, string modeCode)
      {
         int umbracoId = 0;

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         content.Id = 0;
         content.Name = name;
         content.ParentId = UMBRACO_NODE_ID_NATIONAL_PROGRAM_MAIN;
         content.DocType = "NationalProgramGroup";
         content.Template = "NationalProgramGroup";

         List<ApiProperty> properties = new List<ApiProperty>();

         string bodyText = GetProductionPage("/pandp/locations/NPSLocation.htm?modecode=" + modeCode);

         properties.Add(new ApiProperty("modeCode", modeCode)); // Mode Code
         properties.Add(new ApiProperty("bodyText", bodyText)); // Body Text
         properties.Add(new ApiProperty("navigationCategory", "dbc6a2c8-1e09-4f23-9a91-5823a9729626"));
         properties.Add(new ApiProperty("oldUrl", "/pandp/locations/NPSLocation.htm?modecode=" + modeCode)); // current URL               

         content.Properties = properties;

         content.Save = 1;

         request.ContentList = new List<ApiContent>();
         request.ContentList.Add(content);

         //AddLog("Saving area in Umbraco: '" + content.Name + "'...");

         ApiResponse responseBack = ApiCalls.PostData(request, "Post");

         if (responseBack != null)
         {
            if (responseBack.ContentList != null)
            {
               foreach (ApiContent responseContent in responseBack.ContentList)
               {
                  AddLog(" - Save success: " + responseContent.Success);

                  if (true == responseContent.Success)
                  {
                     //AddLog(" - Content Umbraco Id: " + responseContent.Id);
                     //AddLog(" - Node Name: " + responseContent.Name);

                     umbracoId = responseContent.Id;
                  }
                  else
                  {
                     AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                  }

                  //AddLog("");
               }
            }
         }

         return umbracoId;
      }


      static List<NationalProgramGroup> GetNationalProgramGroups()
      {
         List<NationalProgramGroup> nationalProgramGroupList = new List<NationalProgramGroup>();

         nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Nutrition, Food Safety/Quality", NPGroupId = 4, UmbracoId = 0, ModeCode = "02-04-00-00" });
         nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Animal Production and Protection", NPGroupId = 1, UmbracoId = 0, ModeCode = "02-08-00-00" });
         nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Crop Production and Protection", NPGroupId = 3, UmbracoId = 0, ModeCode = "02-06-00-00" });
         nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Natural Resources and Sustainable Agricultural Systems", NPGroupId = 2, UmbracoId = 0, ModeCode = "02-02-00-00" });

         return nationalProgramGroupList;
      }


      static List<NationalProgramGroup> CreateNationalProgramGroups()
      {
         List<NationalProgramGroup> nationalProgramGroupList = GetNationalProgramGroups();

         AddLog("Creating National Program Groups...");

         foreach (NationalProgramGroup group in nationalProgramGroupList)
         {
            AddLog(" - " + group.Name + " (Mode Code: " + group.ModeCode + ")");
            group.UmbracoId = GenerateNationalProgramGroup(group.Name, group.ModeCode);
            AddLog(" - Saved: Umbraco ID: " + group.UmbracoId);
            AddLog("");
         }

         AddLog("Publishing National Program Groups...");

         ApiRequest requestPublish = new ApiRequest();
         ApiContent contentPublish = new ApiContent();

         requestPublish.ApiKey = API_KEY;

         contentPublish.Id = UMBRACO_NODE_ID_NATIONAL_PROGRAM_MAIN;

         requestPublish.ContentList = new List<ApiContent>();
         requestPublish.ContentList.Add(contentPublish);

         ApiResponse responseBack = ApiCalls.PostData(requestPublish, "PublishWithChildren");

         if (responseBack != null)
         {
            AddLog(" - Success: " + responseBack.Success);
            AddLog(" - Message: " + responseBack.Message);
         }


         return nationalProgramGroupList;
      }


      static string StripHTML(string htmlString)

      {

         //This pattern Matches everything found inside html tags;

         //(.|\n) - > Look for any character or a new line

         // *?  -> 0 or more occurences, and make a non-greedy search meaning

         //That the match will stop at the first available '>' it sees, and not at the last one

         //(if it stopped at the last one we could have overlooked 

         //nested HTML tags inside a bigger HTML tag..)





         //  string pattern = @"<(.|\n)*?>";
         //  string oldBulletedListString = htmlString;

         //  return Regex.Replace(htmlString, pattern, string.Empty);


         // start by completely removing all unwanted tags 
         htmlString = Regex.Replace(htmlString, @"<[/]?(font|span|xml|del|ins|[ovwxp]:\w+)[^>]*?>", "", RegexOptions.IgnoreCase);
         // then run another pass over the html (twice), removing unwanted attributes 
         htmlString = Regex.Replace(htmlString, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
         htmlString = Regex.Replace(htmlString, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);

         return htmlString;

      }
      static string StripHTMLAdditional(string htmlString)

      {
         List<string> list = new List<string>(
                            htmlString.Split(new string[] { "\r\n" },
                            StringSplitOptions.RemoveEmptyEntries));
         //string nbspspan8 = "< SPAN style = \"FONT-FAMILY: 'Times New Roman','serif'; FONT-SIZE: 8.5pt; mso-fareast-font-family: Symbol\" > &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </ SPAN >";
         //string nbspspan9 = "< SPAN style = \"FONT-FAMILY: 'Times New Roman','serif'; FONT-SIZE: 8.5pt; mso-fareast-font-family: Symbol\" > &nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </ SPAN >";
         string nbspspan8 = "<P  >Â·&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
         string nbspspan9 = "<P  >Â·&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp";
         string paraEnd = "</P>";
         string ulliBegin = "<P  ><ul type=\"square\"><li>";
         string ulliEnd = "</li></ul></P>";

         //foreach (string s in  list)
         //{
         //    if(s.Contains(nbspspan8))
         //    {
         //        s.Replace(nbspspan8, ulliBegin);
         //        s.Replace(paraEnd, ulliEnd);

         //    }

         //    if (s.Contains(nbspspan9))
         //    {
         //        s.Replace(nbspspan8, ulliBegin);
         //        s.Replace(paraEnd, ulliEnd);
         //    }
         //}

         for (int i = 0; i < list.Count; i++)
         {
            if (list[i].Contains(nbspspan8))
            {
               list[i] = list[i].Replace(nbspspan8, ulliBegin);
               list[i] = list[i].Replace(paraEnd, ulliEnd);

            }

            if (list[i].Contains(nbspspan9))
            {
               list[i] = list[i].Replace(nbspspan8, ulliBegin);
               list[i] = list[i].Replace(paraEnd, ulliEnd);
            }
         }

         htmlString = String.Join(String.Empty, list.ToArray()); ;


         return htmlString;
      }

      static string GetProductionPage(string url, bool usePreviewSite = true)
      {
         string output = "";
         string urlAddress = "";

         if (false == usePreviewSite)
         {
            urlAddress = "http://www.ars.usda.gov" + url;
         }
         else
         {
            urlAddress = "http://iapreview.ars.usda.gov" + url;
         }

         try
         {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
               Stream receiveStream = response.GetResponseStream();
               StreamReader readStream = null;

               if (response.CharacterSet == null)
               {
                  readStream = new StreamReader(receiveStream);
               }
               else
               {
                  readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
               }

               string html = readStream.ReadToEnd();

               response.Close();
               readStream.Close();


               if (false == string.IsNullOrEmpty(html))
               {
                  string between = null;

                  Match m2 = Regex.Match(html, @"(<!\-\- spotlights begin \-\->)(.)*(<!\-\- spotlights end \-\->)", RegexOptions.Singleline);
                  if (m2.Success)
                  {
                     between = m2.Groups[0].Value;
                  }

                  if (false == string.IsNullOrEmpty(between))
                  {
                     output = between;

                     output = CleanHtml.CleanUpHtml(output);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            AddLog("!!! Can't get website page. !!!" + url);
         }

         return output;
      }



      static void AddLog(string line)
      {
         Debug.WriteLine(line);
         Console.WriteLine(line);
         LOG_FILE_TEXT += line + "\r\n";
      }

   }
}
