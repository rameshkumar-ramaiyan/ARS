using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using System.Runtime.Serialization;
using System.Xml.Serialization;

using System.Data.SqlClient;

using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class AddRetrieveLocationsDL
    {
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        public static DataTable GetAllAreas()
        {
            Locations locationsResponse = new Locations();
            string sql = "uspgetAllAreas";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

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
        public static DataTable GetAllCitiesOld(int parentAreaModeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "uspgetAllCities";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);

                sqlComm.Parameters.AddWithValue("@ParentAreaModeCode", parentAreaModeCode);
                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                //foreach (DataRow dr in dt.Rows)
                //{
                //    locationsResponse.LocationModeCode = dr["MODECODE_2"].ToString();
                //    locationsResponse.LocationName = dr["MODECODE_2_DESC"].ToString();



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
        public static DataTable GetAllCities(int parentAreaModeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "uspgetAllCities";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);

                sqlComm.Parameters.AddWithValue("@ParentAreaModeCode", parentAreaModeCode);
                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                //foreach (DataRow dr in dt.Rows)
                //{
                //    locationsResponse.LocationModeCode = dr["MODECODE_2"].ToString();
                //    locationsResponse.LocationName = dr["MODECODE_2_DESC"].ToString();



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

        public static DataTable GetAllResearchUnits(int parentAreaModeCode, int parentCityModeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "uspgetAllResearchUnits";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);

                sqlComm.Parameters.AddWithValue("@ParentAreaModeCode", parentAreaModeCode);
                sqlComm.Parameters.AddWithValue("@ParentCityModeCode", parentCityModeCode);
               
                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                



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

        public static DataTable GetAllResearchCenters(int parentAreaModeCode, int parentCityModeCode, int parentResearchCenterModeCode)
        {
            Locations locationsResponse = new Locations();
            string sql = "uspgetAllResearchCenters";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);

                sqlComm.Parameters.AddWithValue("@ParentAreaModeCode", parentAreaModeCode);
                sqlComm.Parameters.AddWithValue("@ParentCityModeCode", parentCityModeCode);
                sqlComm.Parameters.AddWithValue("@ParentResearchCenterModeCode", parentResearchCenterModeCode);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                //foreach (DataRow dr in dt.Rows)
                //{
                //    locationsResponse.LocationModeCode = dr["MODECODE_2"].ToString();
                //    locationsResponse.LocationName = dr["MODECODE_2_DESC"].ToString();



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