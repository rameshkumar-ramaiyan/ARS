using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.SiteDescendantsAudit.Models;

namespace USDA_ARS.SiteDescendantsAudit.Controllers
{
   [PluginController("SiteDescendantsAudit")]
   [IsBackOffice]
   public class SiteDescendantsAuditApiController : UmbracoAuthorizedJsonController
   {

      [System.Web.Http.HttpGet]
      public IEnumerable<AuditRecordModel> GetAudit(Guid id, string aliases)
      {
         if (true == string.IsNullOrEmpty(aliases) || aliases == "null")
         {
            aliases = "DocsFolder,DocsFolderWithFiles,SitesCareers,SitesNews,PeopleFolder,PersonSite,SiteStandardWebpage";
         }

         var parent = Services.ContentService.GetById(id);
         List<AuditRecordModel> records = null;

         if (parent != null)
         {
            var children = Services.ContentService.GetChildren(parent.Id);

            if (children != null)
            {
               records = GetDescendantAuditRecords(parent.Id, aliases);
            }
         }
         return records;
      }

      private List<AuditRecordModel> GetDescendantAuditRecords(int parentId, string aliases, List<AuditRecordModel> records = null)
      {
         records = records ?? new List<AuditRecordModel>();

         var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

         IEnumerable<IPublishedContent> children = umbracoHelper.TypedContent(parentId).Children;

         foreach (IPublishedContent child in children)
         {
            var db = DatabaseContext.Database;
            //var action = db.Query<string>("SELECT a.[comment] FROM (SELECT Max([Datestamp]) as [date],[logComment] as [comment] FROM [umbracoLog] WHERE NodeId = @0 GROUP BY [logComment]) a", child.Id);

            if (String.IsNullOrEmpty(aliases) || aliases == "null" || aliases.Contains(child.ContentType.Alias))
            {
               records.Add(new AuditRecordModel()
               {
                  Name = child.Name,
                  CreatedBy = Services.UserService.GetProfileById(child.CreatorId).Name,
                  CreatedDate = child.CreateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                  ModifiedBy = Services.UserService.GetProfileById(child.WriterId).Name,
                  ModifiedDate = child.UpdateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                  Action = "",
                  Id = child.Id,
                  ParentId = parentId,
                  ContentTypeAlias = child.ContentType.Alias
               });

            }

            if (child != null && aliases.Contains(child.ContentType.Alias))
            {
               records = GetDescendantAuditRecords(child.Id, aliases, records);
            }
         }

         return records;

      }
   }

}
