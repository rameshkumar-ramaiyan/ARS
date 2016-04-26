using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class ModeCodesNew
    {
        public static string GetNewModeCode(string oldModeCode)
        {
            string output = null;
            ModeCodeNew modeCodeNew = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            oldModeCode = oldModeCode.Replace("-", "");

            string where = "OldModecode = '" + oldModeCode + "'";

            sql = new Sql()
             .Select("*")
             .From("NewModecodes")
             .Where(where);

            modeCodeNew = db.Query<ModeCodeNew>(sql).FirstOrDefault();

            if (modeCodeNew != null)
            {
                output = modeCodeNew.ModecodeNew;
            }

            return output;
        }

        public static List<ModeCodeNew> GetAllNewModeCode()
        {
            List<ModeCodeNew> output = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;


            sql = new Sql()
             .Select("*")
             .From("NewModecodes");

            output = db.Query<ModeCodeNew>(sql).ToList();

            return output;
        }

        public static List<ModeCodeNew> GetOldModeCode(string newModeCode)
        {
            List<ModeCodeNew> modeCodeList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            newModeCode = newModeCode.Replace("-", "");

            string where = "NewModecode = '" + newModeCode + "'";

            sql = new Sql()
             .Select("*")
             .From("NewModecodes")
             .Where(where);

            modeCodeList = db.Query<ModeCodeNew>(sql).ToList();

            return modeCodeList;
        }
    }
}
