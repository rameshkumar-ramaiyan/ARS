using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ZetaHtmlCompressor;
namespace USDA_ARS.LocationsWebApp.DL
{
    public class PersonSite
    {
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;


        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        /// <summary>
        /// Adds people sites for a particluar region/area or research unit
        /// </summary>
        /// <param name="modeCode"></param>
        /// <returns></returns>
        public static ApiResponse AddPeopleSites(string modeCode)
        {
            System.Data.DataTable legacyPeopleBeforeInsertion = new System.Data.DataTable();
            legacyPeopleBeforeInsertion = GetAllPersonsBasedOnModeCode(modeCode);
            System.Data.DataTable newPeopleAfterInsertion = new System.Data.DataTable();
            newPeopleAfterInsertion.Columns.Add("ModeCode");
            newPeopleAfterInsertion.Columns.Add("PersonId");
            newPeopleAfterInsertion.Columns.Add("PersonName");
            newPeopleAfterInsertion.Columns.Add("DocPageContent");
            System.Data.DataTable legacyDocsBeforeInsertion = new System.Data.DataTable();




            ApiResponse responsePage = new ApiResponse();

            // Get the umbraco page by the mode code (Region/Area or Research Unit)
            responsePage = GetCalls.GetNodeByModeCode(modeCode);

            if (responsePage != null && true == responsePage.Success && responsePage.ContentList != null && responsePage.ContentList.Count > 0)
            {
                ApiContent peopleFolder = responsePage.ContentList[0].ChildContentList.Where(p => p.DocType == "PeopleFolder").FirstOrDefault();

                if (peopleFolder != null)
                {
                    List<ApiContent> contentPeopleSites = new List<ApiContent>();


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
                                personSiteHtml = replaceSP2withARS(personSiteHtml);
                                personSiteHtml = DL.CleanHtml.CleanUpHtml(personSiteHtml);
                            }

                        }


                        //int personId = 42111;
                        //string personName = "Colin S. Brent";
                        //string  personSiteHtml = "<p>Hello!</p>";

                        ApiContent personSite = GeneratePersonSiteContent(peopleFolder.Id, personId, personName, personSiteHtml);

                        if (personSite != null)
                        {
                            contentPeopleSites.Add(personSite);
                        }
                    } // END LOOP

                    ApiRequest request = new ApiRequest();

                    request.ContentList = contentPeopleSites;
                    request.ApiKey = API_KEY;

                    ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                    return responseBack;
                }
                else
                {
                    //  ApiResponse responsePage1 = new ApiResponse();
                    //  return responsePage1;
                    throw new Exception("Unable to find Umbraco People Folder within Site: " + responsePage.ContentList[0].Name);
                }

            }
            else
            {
                ApiResponse responsePage2 = new ApiResponse();
                return responsePage2;
                // throw new Exception("Unable to find Umbraco page by Mode Code: " + modeCode);
            }
        }


        /// <summary>
        /// Generates a ApiContent object of person site for API
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="personId"></param>
        /// <param name="name"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static ApiContent GeneratePersonSiteContent(int parentId, int personId, string name, string body)
        {
            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = "PersonSite";
            content.Template = "PersonSite"; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
            properties.Add(new ApiProperty("personLink", personId)); // Person's ID                                                                                         
            properties.Add(new ApiProperty("oldUrl", "/pandp/people/people.htm?personid=" + personId)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            return content;
        }
        #region  Get All Persons based on Mode Code
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

        public static string CleanUpHtml(string bodyText)
        {
            string output = "";

            if (false == string.IsNullOrEmpty(bodyText))
            {
                HtmlCompressor htmlCompressor = new HtmlCompressor();
                htmlCompressor.setRemoveMultiSpaces(true);
                htmlCompressor.setRemoveIntertagSpaces(true);

                output = htmlCompressor.compress(bodyText);
            }

            return output;
        }
        public static string replaceSP2withARS(string personSiteHtml)
        {
            if (containsExtension.CaseInsensitiveContains(personSiteHtml, "sp2UserFiles/person/"))
            {


                string result =
                   Regex.Replace(personSiteHtml, "sp2UserFiles/person/", "ARSUserFiles/", RegexOptions.IgnoreCase);


                personSiteHtml = result;
            }
            return personSiteHtml;
        }
        #endregion

    }
    public static class containsExtension
    {
        public static bool CaseInsensitiveContains(this string text, string value,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}
