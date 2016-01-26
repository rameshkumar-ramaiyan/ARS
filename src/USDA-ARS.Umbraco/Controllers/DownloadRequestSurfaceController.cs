using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Controllers
{
    public class DownloadRequestSurfaceController : SurfaceController
    {
        // GET: DownloadRequest
        public ActionResult Index()
        {
            DownloadRequest downloadRequest = new DownloadRequest();
            string modeCode = "";

            if (false == string.IsNullOrWhiteSpace(Request.QueryString["modeCode"]))
            {
                downloadRequest.ModeCode = Request.QueryString.Get("modeCode");
            }
            if (false == string.IsNullOrWhiteSpace(Request.QueryString["softwareid"]))
            {
                downloadRequest.SoftwareId = Request.QueryString.Get("softwareId");
            }

            if (false == string.IsNullOrEmpty(downloadRequest.SoftwareId))
            {
                return PartialView("DownloadRequest", downloadRequest);
            }
            else
            {
                return Redirect("/services/software/?modeCode=" + modeCode);
            }

        }

        [NotChildAction]
        [HttpPost]
        public ActionResult HandleFormSubmit(DownloadRequest model)
        {
            if (false == ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }
            else
            {
                IPublishedContent sitePage = USDA_ARS.Umbraco.Extensions.Helpers.Nodes.GetNodeByModeCode(model.ModeCode);

                ArchetypeModel softwareList = sitePage.GetPropertyValue<ArchetypeModel>("software");

                if (softwareList != null && softwareList.Any())
                {
                    Guid softwareIdGuid = Guid.Parse(model.SoftwareId);
                    ArchetypeFieldsetModel softwareItem = softwareList.Where(p => p.Id == softwareIdGuid).FirstOrDefault();

                    List<string> recipientsArray = new List<string>();

                    if (softwareItem != null)
                    {
                        Extensions.Models.Aris.DownloadRequest downloadToSave = new Extensions.Models.Aris.DownloadRequest();

                        downloadToSave.SoftwareId = Guid.Parse(model.SoftwareId);
                        downloadToSave.FirstName = model.FirstName;
                        downloadToSave.LastName = model.LastName;
                        downloadToSave.MiddleName = model.MiddleName;
                        downloadToSave.Email = model.Email;
                        downloadToSave.Affiliation = model.Affiliation;
                        downloadToSave.Purpose = model.Purpose;
                        downloadToSave.Comments = model.Comments;
                        downloadToSave.TimeStamp = DateTime.Now;
                        downloadToSave.RemoteAddr = Request.UserHostAddress;
                        downloadToSave.City = model.City;
                        downloadToSave.State = model.State;
                        downloadToSave.Country = model.Country;
                        downloadToSave.Reference = model.Reference;

                        Extensions.Helpers.Aris.DownloadRequest.SaveDownloadRequest(downloadToSave);

                        if (softwareItem.HasValue("recipients"))
                        {
                            recipientsArray = softwareItem.GetValue<string>("recipients").Split(',').ToList();




                            //TODO: Send Email

                        }

                        TempData["downloadReady"] = true;

                        return new RedirectResult("/services/software/download?modeCode="+ model.ModeCode + "&softwareid="+ Server.UrlEncode(model.SoftwareId));
                    }
                }


                return CurrentUmbracoPage();
            }
        }
    }
}