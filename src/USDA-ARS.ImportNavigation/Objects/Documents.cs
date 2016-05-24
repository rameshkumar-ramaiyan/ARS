using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNavigation.Models;

namespace USDA_ARS.ImportNavigation.Objects
{
    public class Documents
    {
        public static Document GetDocById(int docId)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM Documents WHERE DocId = @docId";

            Document docItem = db.Query<Document>(sql, new { docId = docId }).FirstOrDefault();

            return docItem;
        }
    }
}
