using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class GetCalls
    {
        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        /// <summary>
        /// Gets an Umbraco page by mode code
        /// </summary>
        /// <param name="modeCode"></param>
        /// <returns></returns>
        public static ApiResponse GetNodeByModeCode(string modeCode)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            request.ContentList = new List<ApiContent>();
            content.Properties = new List<ApiProperty>();

            // Get the Umbraco page by Mode Code
            content.Properties.Add(new ApiProperty("modeCode", modeCode)); // Region mode code

            request.ContentList.Add(content);

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = ApiCalls.PostData(request, "Get");

            return responseBack;
        }

        public static ApiResponse GetNodeByNationalProgramCode(string npCode)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            request.ContentList = new List<ApiContent>();
            content.Properties = new List<ApiProperty>();

            // Get the Umbraco page by Mode Code
            content.Properties.Add(new ApiProperty("npCode", npCode)); // Region mode code

            request.ContentList.Add(content);

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = ApiCalls.PostData(request, "Get");

            return responseBack;
        }
    }
}