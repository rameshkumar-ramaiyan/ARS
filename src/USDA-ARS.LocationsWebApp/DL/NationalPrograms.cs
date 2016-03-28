using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class NationalPrograms
    {
        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        public static void ImportNationPrograms()
        {
            //
        }


        


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