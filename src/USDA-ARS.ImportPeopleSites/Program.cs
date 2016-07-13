using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportPeopleSites.Models;
using USDA_ARS.ImportPeopleSites.Objects;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportPeopleSites
{
    class Program
    {
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

        static string LOG_FILE_TEXT = "";

        static DateTime TIME_STARTED = DateTime.MinValue;
        static DateTime TIME_ENDED = DateTime.MinValue;

        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static List<PeopleFolderLookup> PEOPLE_FOLDER_LIST = null;

        #region modified main 
        static void Main(string[] args)
        {
            bool forceCacheUpdate = false;

            if (args != null && args.Length >= 1)
            {
                if (args[0] == "force-cache-update")
                {
                    forceCacheUpdate = true;
                }
            }

            Logs.AddLog(ref LOG_FILE_TEXT, "-= ADDING PEOPLE SITES =-");
            Logs.AddLog(ref LOG_FILE_TEXT, "");
            Logs.AddLog(ref LOG_FILE_TEXT, "");

            try
            {

                TIME_STARTED = DateTime.Now;

                Logs.AddLog(ref LOG_FILE_TEXT, "Getting Mode Codes From Umbraco...");
                ModeCodes.GenerateModeCodeList(ref MODE_CODE_LIST, ref LOG_FILE_TEXT, forceCacheUpdate);
                Logs.AddLog(ref LOG_FILE_TEXT, "Done. Count: " + MODE_CODE_LIST.Count);

                Logs.AddLog(ref LOG_FILE_TEXT, "");

                Logs.AddLog(ref LOG_FILE_TEXT, "Getting People Folders From Umbraco...");
                PeopleFolders.GenerateModeCodeFolderList(ref PEOPLE_FOLDER_LIST, ref LOG_FILE_TEXT, MODE_CODE_LIST, forceCacheUpdate);
                Logs.AddLog(ref LOG_FILE_TEXT, "Done. Count: " + PEOPLE_FOLDER_LIST.Count);

                Logs.AddLog(ref LOG_FILE_TEXT, "");


                if (PEOPLE_FOLDER_LIST != null && PEOPLE_FOLDER_LIST.Any())
                {
                    // LOOP THROUGH VALID MODE CODES
                    foreach (PeopleFolderLookup peopleFolder in PEOPLE_FOLDER_LIST)
                    {
                        string modeCode = peopleFolder.ModeCode;

                        if (peopleFolder != null)
                        {
                            Logs.AddLog(ref LOG_FILE_TEXT, "Mode Code: " + modeCode);

                            int peopleFolderUmbracoId = peopleFolder.PeopleFolderUmbracoId;

                            System.Data.DataTable legacyPeopleBeforeInsertion = new System.Data.DataTable();
                            legacyPeopleBeforeInsertion = GetAllPersonsBasedOnModeCode(modeCode);
                            System.Data.DataTable newPeopleAfterInsertion = new System.Data.DataTable();
                            newPeopleAfterInsertion.Columns.Add("ModeCode");
                            newPeopleAfterInsertion.Columns.Add("PersonId");
                            newPeopleAfterInsertion.Columns.Add("PersonName");
                            newPeopleAfterInsertion.Columns.Add("DocPageContent");
                            System.Data.DataTable legacyDocsBeforeInsertion = new System.Data.DataTable();

                            bool peopleSiteAdded = false;

                            // ADD PEOPLE SITES HERE: (LOOP)
                            for (int i = 0; i < legacyPeopleBeforeInsertion.Rows.Count; i++)
                            {
                                string completeModeCode = legacyPeopleBeforeInsertion.Rows[i].Field<string>(0);
                                int personId = 0;
                                string personName = "";
                                string personSiteHtml = "";
                                if (!string.IsNullOrEmpty(legacyPeopleBeforeInsertion.Rows[i].Field<string>(1)))
                                {
                                    personId = int.Parse(legacyPeopleBeforeInsertion.Rows[i].Field<string>(1).Trim());
                                    personName = legacyPeopleBeforeInsertion.Rows[i].Field<string>(2);
                                }
                                //call sp to get doc ids and documents
                                legacyDocsBeforeInsertion = GetAllDocumentIdsBasedOnPersonId(personId.ToString());
                                if (legacyDocsBeforeInsertion != null)
                                {
                                    for (int j = 0; j < legacyDocsBeforeInsertion.Rows.Count; j++)
                                    {
                                        personSiteHtml = legacyDocsBeforeInsertion.Rows[j].Field<string>(0);
                                        //personSiteHtml = replaceSP2withARS(personSiteHtml);
                                        personSiteHtml = CleanHtml.CleanUpHtml(personSiteHtml);
                                    }
                                }

                                // Make sure the HTML is not empty
                                if (false == string.IsNullOrWhiteSpace(personSiteHtml))
                                {
                                    Logs.AddLog(ref LOG_FILE_TEXT, " - Adding Person: " + personName);
                                    ApiResponse apiResponse = AddUmbracoPersonPage(peopleFolderUmbracoId, personId, personName, personSiteHtml);

                                    if (apiResponse != null && apiResponse.Success)
                                    {
                                        Logs.AddLog(ref LOG_FILE_TEXT, " - Added Person (" + personId + "): " + personName);

                                        peopleSiteAdded = true;
                                    }
                                    else
                                    {
                                        Logs.AddLog(ref LOG_FILE_TEXT, " - !ERROR! Person not added (" + personId + "): " + personName + " | " + apiResponse.Message);
                                    }
                                }
                            }

                            if (true == peopleSiteAdded)
                            {
                                Logs.AddLog(ref LOG_FILE_TEXT, "Publishing People Sites for '" + peopleFolder.ModeCode + "'...");

                                ApiRequest requestPublish = new ApiRequest();
                                ApiContent contentPublish = new ApiContent();

                                requestPublish.ApiKey = API_KEY;

                                contentPublish.Id = peopleFolder.PeopleFolderUmbracoId;

                                requestPublish.ContentList = new List<ApiContent>();
                                requestPublish.ContentList.Add(contentPublish);

                                ApiResponse responseBackPublish = ApiCalls.PostData(requestPublish, "PublishWithChildren");

                                if (responseBackPublish != null)
                                {
                                    Logs.AddLog(ref LOG_FILE_TEXT, " - Success: " + responseBackPublish.Success);
                                    Logs.AddLog(ref LOG_FILE_TEXT, " - Message: " + responseBackPublish.Message);
                                }

                            }
                        } // END LOOP



                        //// ADD PEOPLE SITES HERE: (LOOP THROUGH VALID/ACTIVE PEOPLE IN THE MODE CODE)
                        //{
                        //    int personId = 0; // GET PERSON ID
                        //    string personName = ""; //GET PERSON NAME
                        //    string personSiteHtml = ""; // GET PERSON SITE HTML

                        //    personSiteHtml = CleanHtml.CleanUpHtml(personSiteHtml);

                        //    // Make sure the HTML is not empty
                        //    if (false == string.IsNullOrWhiteSpace(personSiteHtml))
                        //    {
                        //        ApiResponse apiResponse = AddUmbracoPersonPage(peopleFolderUmbracoId, personId, personName, personSiteHtml);

                        //        if (apiResponse != null && apiResponse.Success)
                        //        {
                        //            Logs.AddLog(ref LOG_FILE_TEXT, " - Added Person (" + personId + "): " + personName);
                        //        }
                        //        else
                        //        {
                        //            Logs.AddLog(ref LOG_FILE_TEXT, " - !ERROR! Person not added (" + personId + "): " + personName + " | " + apiResponse.Message);
                        //        }
                        //    }
                        //}
                    }
                }


                Logs.AddLog(ref LOG_FILE_TEXT, "");
                Logs.AddLog(ref LOG_FILE_TEXT, "");
                Logs.AddLog(ref LOG_FILE_TEXT, "");
                Logs.AddLog(ref LOG_FILE_TEXT, "/// IMPORT COMPLETE ///");
                Logs.AddLog(ref LOG_FILE_TEXT, "");

                TIME_ENDED = DateTime.Now;

                TimeSpan timeLength = TIME_ENDED.Subtract(TIME_STARTED);

                Logs.AddLog(ref LOG_FILE_TEXT, "/// Time to complete: " + timeLength.ToString(@"hh") + " hours : " + timeLength.ToString(@"mm") + " minutes : " + timeLength.ToString(@"ss") + " seconds ///");
            }
            catch (Exception ex)
            {
                Logs.AddLog(ref LOG_FILE_TEXT, "!!!!!!! ERROR !!!!!!!" + ex.ToString());
            }

            using (FileStream fs = File.Create("LOG_FILE.txt"))
            {
                // Add some text to file
                Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
                fs.Write(fileText, 0, fileText.Length);
            }
        }

        #endregion
        static ApiResponse AddUmbracoPersonPage(int parentId, int personId, string name, string body)
        {
            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = "PersonSite";
            content.Template = "PersonSite";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
            properties.Add(new ApiProperty("personLink", personId)); // Person's ID                                                                                         
            properties.Add(new ApiProperty("oldUrl", "/pandp/people/people.htm?personid=" + personId)); // current URL               

            content.Properties = properties;

            content.Save = 1;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }

        public static DataTable GetAllPersonsBasedOnModeCode(string modeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "[uspgetAllPersonsBasedOnModeCode]";
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
        public static DataTable GetAllDocumentIdsBasedOnPersonId(string personId)
        {
            Locations locationsResponse = new Locations();
            string sql = "[uspgetAllDocumentIdsBasedOnPersonId]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlComm.Parameters.AddWithValue("@PersonId", personId);

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

    }
}
