using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
   public class Locations
   {
      /// <summary>
      /// Get the Location Name via the Aris_Public_Web DB
      /// </summary>
      /// <param name="modeCode"></param>
      /// <returns></returns>
      public static string GetLocationNameByModeCode(string modeCode)
      {
         string locationName = null;

         Location locationItem = GetLocationObjectByModeCode(modeCode);

         if (locationItem != null)
         {
            if (false == string.IsNullOrWhiteSpace(locationItem.ModeCode4Desc))
            {
               locationName = locationItem.ModeCode4Desc;
            }
            else if (false == string.IsNullOrWhiteSpace(locationItem.ModeCode3Desc))
            {
               locationName = locationItem.ModeCode3Desc;
            }
            else if (false == string.IsNullOrWhiteSpace(locationItem.ModeCode2Desc))
            {
               locationName = locationItem.ModeCode2Desc;
            }
            else if (false == string.IsNullOrWhiteSpace(locationItem.ModeCode1Desc))
            {
               locationName = locationItem.ModeCode1Desc;
            }
         }

         return locationName;
      }


      /// <summary>
      /// Get the location object from the Aris_Public_Web DB using the Mode Code
      /// </summary>
      /// <param name="modeCode"></param>
      /// <returns></returns>
      public static Location GetLocationObjectByModeCode(string modeCode)
      {
         Location locationItem = null;
         List<Location> locationList = null;

         modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

         if (false == string.IsNullOrWhiteSpace(modeCode))
         {
            locationList = GetAllLocations();

            if (locationList != null && locationList.Any())
            {
               locationItem = locationList.Where(p => p.ModeCodeConcat == modeCode).FirstOrDefault();
            }
         }

         return locationItem;
      }


      /// <summary>
      /// Gets the level of the location 01-01-00-00 = level 3, 01-01-01-00 = level 4
      /// </summary>
      /// <param name="location"></param>
      /// <returns></returns>
      public static int LocationLevel(Location location)
      {
         int level = -1;

         if (location != null)
         {
            if (location.ModeCode2 == "00" && location.ModeCode3 == "00" && location.ModeCode4 == "00")
            {
               level = 1;
            }
            else if (location.ModeCode3 == "00" && location.ModeCode4 == "00")
            {
               level = 2;
            }
            else if (location.ModeCode4 == "00")
            {
               level = 3;
            }
            else
            {
               level = 4;
            }
         }

         return level;
      }


      /// <summary>
      /// Get all locations from Aris_Public_Web DB (cached)
      /// </summary>
      /// <returns></returns>
      public static List<Location> GetAllLocations()
      {
         List<Location> locationList = null;
         string cacheKey = "LocationList";
         int cacheUpdateInMinutes = 720;

         ObjectCache cache = MemoryCache.Default;

         locationList = cache.Get(cacheKey) as List<Location>;

         if (locationList == null)
         {
            var db = new Database("arisPublicWebDbDSN");

            string sql = "SELECT * FROM v_locations2";

            locationList = db.Query<Location>(sql).ToList();

            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
            cache.Add(cacheKey, locationList, policy);
         }

         return locationList;
      }


      public static List<Location> GetChildLocationsList(string modeCode)
      {
         List<Location> locationList = null;

         List<string> modeCodeArray = ModeCodes.ModeCodeArray(modeCode);

         if (modeCodeArray != null && modeCodeArray.Any())
         {
            List<Location> locationListAll = GetAllLocations();

            if (modeCodeArray[1] == "00")
            {
               locationList = locationListAll.Where(p => p.ModeCode1 == modeCodeArray[0] && p.ModeCode2 != "00").ToList();
            }
            else if (modeCodeArray[2] == "00")
            {
               locationList = locationListAll.Where(p => p.ModeCode1 == modeCodeArray[0] && p.ModeCode2 == modeCodeArray[1] && p.ModeCode3 != "00").ToList();
            }
            else if (modeCodeArray[3] == "00")
            {
               locationList = locationListAll.Where(p => p.ModeCode1 == modeCodeArray[0] && p.ModeCode2 == modeCodeArray[1] && p.ModeCode3 == modeCodeArray[2] &&
                     p.ModeCode4 != "00").ToList();
            }
         }


         return locationList;
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="modeCode"></param>
      /// <returns></returns>
      public static List<Location> GetAncestorLocationsList(string modeCode)
      {
         List<Location> locationList = null;

         List<string> modeCodeArray = ModeCodes.ModeCodeArray(modeCode);

         if (modeCodeArray != null && modeCodeArray.Any())
         {
            locationList = new List<Location>();

            List<Location> locationListAll = GetAllLocations();

            if (modeCodeArray[3] != "00")
            {
               Location addLocation = GetLocationObjectByModeCode(modeCodeArray[0] + modeCodeArray[1] + modeCodeArray[2] + "00");
               if (addLocation != null)
               {
                  addLocation.Level = 3;
                  locationList.Add(addLocation);
               }
            }
            if (modeCodeArray[2] != "00")
            {
               Location addLocation = GetLocationObjectByModeCode(modeCodeArray[0] + modeCodeArray[1] + "0000");
               if (addLocation != null)
               {
                  addLocation.Level = 2;
                  locationList.Add(addLocation);
               }
            }
            if (modeCodeArray[1] != "00")
            {
               Location addLocation = GetLocationObjectByModeCode(modeCodeArray[0] + "000000");
               if (addLocation != null)
               {
                  addLocation.Level = 1;
                  locationList.Add(addLocation);
               }
            }

            if (locationList != null && locationList.Any())
            {
               locationList = locationList.OrderBy(p => p.Level).ToList();
            }
         }

         return locationList;
      }
   }
}
