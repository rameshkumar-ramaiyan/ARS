using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class DownloadRequests
    {
        public static List<Models.Aris.DownloadRequest> GetDownloadRequests(Guid softwareId)
        {
            List<Models.Aris.DownloadRequest> downloadList = new List<Models.Aris.DownloadRequest>();

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            string where = "SoftwareId = '" + softwareId.ToString() + "'";

            sql = new Sql()
             .Select("*")
             .From("DownloadRequests")
             .Where(where);

            downloadList = db.Query<Models.Aris.DownloadRequest>(sql).ToList();

            return downloadList;
        }


        public static Models.NodeDownloadRequests GetDownloadRequestsByNode(IPublishedContent node)
        {
            Models.NodeDownloadRequests nodeDownloadRequests = new Models.NodeDownloadRequests();

            var db = new Database("arisPublicWebDbDSN");

            ArchetypeModel softwareList = node.GetPropertyValue<ArchetypeModel>("software");

            if (softwareList != null & softwareList.Any())
            {
                nodeDownloadRequests.NodeDownloadRequestsList = new List<Models.NodeDownloadRequestsList>();

                foreach (var softwareItem in softwareList)
                {
                    Models.NodeDownloadRequestsList nodeDownloadList = new Models.NodeDownloadRequestsList();

                    if (softwareItem.HasValue("title"))
                    {
                        nodeDownloadList.SoftwareTitle = softwareItem.GetValue<string>("title");
                    }

                    List<Models.Aris.DownloadRequest> downloadList = new List<Models.Aris.DownloadRequest>();

                    downloadList = GetDownloadRequests(softwareItem.Id);

                    if (downloadList != null && downloadList.Any())
                    {
                        downloadList = downloadList.OrderByDescending(p => p.TimeStamp).ToList();

                        nodeDownloadList.DownloadRequestList = downloadList;
                    }

                    nodeDownloadRequests.NodeDownloadRequestsList.Add(nodeDownloadList);
                }
                
            }

            return nodeDownloadRequests;
        }


        public static bool SaveDownloadRequest(Models.Aris.DownloadRequest downloadRequest)
        {
            bool success = false;

            var db = new Database("arisPublicWebDbDSN");

            if (downloadRequest.Id == 0)
            {
                db.Insert(downloadRequest);
            }
            else
            {
                db.Save(downloadRequest);
            }

            

            return success;
        }
    }
}
