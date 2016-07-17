using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class ModeCodeActives
    {
        public static void UpdateModeCodeActiveTable()
        {
            Database db = new Database("umbracoDbDSN");

            string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'modeCode')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar <> '' AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

            List<UmbracoPropertyData> propertyDataList = db.Query<UmbracoPropertyData>(sql).ToList();

            if (propertyDataList != null && propertyDataList.Any())
            {
                DropModeCodeActiveTable();

                foreach (UmbracoPropertyData propertyDataItem in propertyDataList)
                {
                    string modeCodeNoDashes = ModeCodes.ModeCodeNoDashes(propertyDataItem.DataNvarchar);

                    AddModeCodeActiveRecord(new ModeCodeActive() { ModeCode = modeCodeNoDashes, UmbracoId = propertyDataItem.UmbracoId });
                }
            }
                
        }


        public static void DropModeCodeActiveTable()
        {
            Database db = new Database("arisPublicWebDbDSN");

            db.TruncateTable("ModeCodeActive");
        }


        public static void AddModeCodeActiveRecord(ModeCodeActive modeCodeActive)
        {
            Database db = new Database("arisPublicWebDbDSN");

            db.Insert(modeCodeActive);
        }
    }
}
