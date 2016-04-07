﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using System.Data;
using System.Data.SqlClient;
using USDA_ARS.LocationsWebApp.Models;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class NationalPrograms
    {
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        public static void ImportNationPrograms()
        {
            // GET NATIONAL PROGRAM GROUPS
            List<NationalProgramGroup> nationalProgramGroupList = GetNationalProgramGroups();

            foreach (NationalProgramGroup npGroup in nationalProgramGroupList)
            {
                // You can now get the National Program Items by either:
                // Mode Code: npGroup.ModeCode
                // or...
                // NPGroupId: npGroup.NPGroupId

                DataTable legacyNPProgramItems = GetAllNationalProgramItems(npGroup.ModeCode); // The Mode Code must be in xx-xx-xx-xx format

                for (int legacyNPProgramsRowId = 0; legacyNPProgramsRowId < legacyNPProgramItems.Rows.Count; legacyNPProgramsRowId++)
                {
                    // ========================
                    // GET THE INFORMATION FOR THE NATIONAL PROGRAM ITEM
                    // ========================

                    string npTitle = "NATIONAL PROGRAM TITLE";
                    string npCode = "NP_CODE_HERE";
                    string npText = "BODY TEXT OF NATION PROGRAM ITEM";

                    ApiContent content = new ApiContent();

                    content = GenerateNationalProgramItem(npGroup.UmbracoId, npTitle, npCode, npText);

                    ApiRequest request = new ApiRequest();

                    request.ApiKey = API_KEY;

                    request.ContentList = new List<ApiContent>();
                    request.ContentList.Add(content);

                    ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                    if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                    {
                        int umbracoParentId = responseBack.ContentList[0].Id;

                        ApiResponse responseDocsFolder = new ApiResponse();

                        // Get DOCS Folder: The umbraco page by the NP Code
                        responseDocsFolder = GetCalls.GetNodeByModeCode(npCode);

                        if (responseDocsFolder != null && true == responseDocsFolder.Success && responseDocsFolder.ContentList != null && responseDocsFolder.ContentList.Count > 0)
                        {
                            ApiContent docsFolder = responseDocsFolder.ContentList[0].ChildContentList.Where(p => p.DocType == "NationalProgramFolderContainer").FirstOrDefault();

                            if (docsFolder != null)
                            {
                                // We now have the Docs Folder for the National Program Item. Lets get the docs from sitepublisher...

                                List<ApiContent> contentDocPages = new List<ApiContent>(); // Create the ApiContent List for all the docs for a NP item.

                                // Get the Documents by National Program.
                                DataTable legacyNPProgramDocs = GetAllNationalProgramDocuments(npCode); // SHOULD YOU GET BY NP CODE INSTEAD?

                                for (int legacyNPProgramsDocRowId = 0; legacyNPProgramsDocRowId < legacyNPProgramDocs.Rows.Count; legacyNPProgramsDocRowId++)
                                {
                                    // ========================
                                    // GET THE INFORMATION FOR THE NATIONAL PROGRAM ITEM DOCUMENTS
                                    // ========================

                                    string legacyDocType = "SITEPUBLISHER DOC TYPE";// Get the Legacy sitepublisher doctype. THIS NEEDS TO BE: Program Inputs, Program Planning, or Program Reports
                                    string legacyDocTitle = "DOC TITLE HERE";
                                    string legacyDocText = "DOC TITLE HERE";
                                    string oldDocId = "OLD SITEPUBLSHER DOC ID";

                                    string umbracoDocType = "";

                                    if (legacyDocType == "Program Inputs")
                                    {
                                        umbracoDocType = "NPProgramInputs";
                                    }
                                    else if (legacyDocType == "Program Inputs")
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

                                    // Add the doc page to the ApiContent
                                    ApiContent docPage = GenerateNationalProgramDocument(docsFolder.Id, legacyDocTitle, umbracoDocType, legacyDocText, npCode, oldDocId);

                                    if (docPage != null)
                                    {
                                        // Add the doc page to the ApiContent Collection List
                                        contentDocPages.Add(docPage);
                                    }
                                }

                                // POST all the docs for a national program item
                                ApiRequest requestDocs = new ApiRequest();

                                requestDocs.ApiKey = API_KEY;

                                requestDocs.ContentList = new List<ApiContent>();
                                requestDocs.ContentList = contentDocPages;

                                ApiResponse responseBackDocs = ApiCalls.PostData(request, "Post");

                                if (responseBackDocs.ContentList != null && responseBackDocs.ContentList.Count > 0)
                                {
                                    // Success
                                }

                            }
                        }
                    }
                }
            }
        }

        public static string DeleteAllNationalProgramItems()
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

        protected static ApiResponse DeleteNationalProgramNodes(List<ApiContent> contentList)
        {
            ApiResponse response = null;
            ApiRequest request = new ApiRequest();

            request.ApiKey = API_KEY;
            request.ContentList = contentList;

            response = ApiCalls.PostData(request, "Delete");

            return response;
        }

        //public static DataTable GetAllNationalProgramGroups()
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
        public static DataTable GetAllNationalProgramItems(string modeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "[uspgetAllNPPrograms]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlComm.Parameters.AddWithValue("@ModeCode", modeCode);

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
        public static DataTable GetAllNationalProgramDocuments(string modeCode)
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
            string sql = "[uspgetAllNPProgramDocuments]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlComm.Parameters.AddWithValue("@ModeCode", modeCode);

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
        //public static ApiContent GenerateNationalProgramGroup(string name, string modeCode)
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

        public static ApiContent GenerateNationalProgramItem(int parentId, string name, string npCode, string bodyText)
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
            properties.Add(new ApiProperty("oldUrl", "/research/programs/programs.htm?NP_CODE=" + npCode)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            return content;
        }

        //inputs-np code(mode code) , name,body text old url,doc type , parent umbraco id( is this np or npgroup id?)
        //out puts-content id

        public static ApiContent GenerateNationalProgramDocument(int parentId, string name, string docType, string bodyText, string npCode, string oldDocId)
        {
            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = docType;
            content.Template = "NationalProgramDocument";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", bodyText)); // Body Text
            properties.Add(new ApiProperty("oldUrl", "/research/programs/programs.htm?np_code=" + npCode + "&docid=" + oldDocId)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            return content;
        }


        public static List<NationalProgramGroup> GetNationalProgramGroups()
        {
            List<NationalProgramGroup> nationalProgramGroupList = new List<NationalProgramGroup>();

            nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Nutrition, Food Safety/Quality", NPGroupId = 4, UmbracoId = 1127, ModeCode = "02-04-00-00" });
            nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Animal Production and Protection", NPGroupId = 1, UmbracoId = 2223, ModeCode = "02-08-00-00" });
            nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Crop Production and Protection", NPGroupId = 3, UmbracoId = 2224, ModeCode = "02-06-00-00" });
            nationalProgramGroupList.Add(new NationalProgramGroup { Name = "Natural Resources and Sustainable Agricultural Systems", NPGroupId = 2, UmbracoId = 2225, ModeCode = "02-02-00-00" });

            return nationalProgramGroupList;
        }


    }
}