using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;
using System.Data;
using System.Data.SqlClient;
namespace USDA_ARS.LocationsWebApp.DL
{
    public class NationalPrograms
    {
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        public static void ImportNationPrograms()
        {
            DataTable legacyNPPrograms = new DataTable();
            legacyNPPrograms = GetAllNationalProgramGroups();
            System.Data.DataTable newNPPrograms = new System.Data.DataTable();
            newNPPrograms.Columns.Add("NPGroupID");            
            newNPPrograms.Columns.Add("NPGTitle");
            newNPPrograms.Columns.Add("NPGAbbr");
           
            for (int legacyNPProgramsRowId = 0; legacyNPProgramsRowId < legacyNPPrograms.Rows.Count; legacyNPProgramsRowId++)
            {
                string NPGroupID = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<int>(0).ToString();string NPNumber = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<int>(1).ToString();
                string NPGTitle = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<string>(1).ToString();
                string NPGAbbr = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<string>(2).ToString();
                ApiContent content = new ApiContent();
                content = GenerateNationalProgramGroup(NPGTitle, NPNumber);

                newNPPrograms.Rows.Add(NPID, NPNumber, NPTitle, NPGroup_ID, content.Id, 1126);

            }



            DataTable legacyNPPrograms = new DataTable();
            legacyNPPrograms = GetAllNationalProgramGroups();
            System.Data.DataTable newNPPrograms = new System.Data.DataTable();
            newNPPrograms.Columns.Add("NPID");
            newNPPrograms.Columns.Add("NPNumber");
            newNPPrograms.Columns.Add("NPTitle");
            newNPPrograms.Columns.Add("NPGroup_ID");
            newNPPrograms.Columns.Add("NPContentID");
            for (int legacyNPProgramsRowId=0; legacyNPProgramsRowId< legacyNPPrograms.Rows.Count;legacyNPProgramsRowId++)
            {
                string NPID = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<int>(0).ToString();
                string NPNumber = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<int>(1).ToString();
                string NPTitle = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<string>(2).ToString();              
                string NPGroup_ID = legacyNPPrograms.Rows[legacyNPProgramsRowId].Field<int>(3).ToString();
                ApiContent content = new ApiContent();
                content=GenerateNationalProgramGroup(NPTitle, NPNumber);

                newNPPrograms.Rows.Add(NPID, NPNumber, NPTitle, NPGroup_ID,content.Id,1126);
                
            }


        }
        public static DataTable GetAllNationalProgramGroups()
        {
            Locations locationsResponse = new Locations();
            string sql = "[uspgetAllNPGroups]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                //sqlComm.Parameters.AddWithValue("@ModeCode", modeCode);

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
        public static ApiContent GenerateNationalProgramGroup(string name, string modeCode)
        {
            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = 1126; // Programs node
            content.DocType = "NationalProgramGroup";
            content.Template = "NationalProgramGroup";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("modeCode", modeCode)); // Mode Code
            properties.Add(new ApiProperty("oldUrl", "/pandp/locations/NPSLocation.htm?modecode=" + modeCode)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            return content;
        }
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
            properties.Add(new ApiProperty("oldUrl", "/research/programs/programs.htm?np_code="+ npCode + "&docid=" + oldDocId)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            return content;
        }



    }
}