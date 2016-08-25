using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using USDA_ARS.Umbraco.Extensions.Helpers;
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
         else
         {
            downloadRequest.ModeCode = "00-00-00-00";
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


      /// <summary>
      /// POST: Download Software Form
      /// </summary>
      /// <param name="model"></param>
      /// <returns></returns>
      [NotChildAction]
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult HandleFormSubmit(DownloadRequest model)
      {
         // Check recaptcha
         var response = Request["g-recaptcha-response"];
         //secret that was generated in key value pair
         string secret = ConfigurationManager.AppSettings["Google:Recaptcha:Secret"];

         var client = new WebClient();
         var reply = client.DownloadString(String.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));

         var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(reply);

         if (false == captchaResponse.Success)
         {
            ModelState.AddModelError("", "The CAPTCHA was invalid.");
         }

         if (false == ModelState.IsValid)
         {
            return CurrentUmbracoPage();
         }
         else
         {
            // If the mode code is empty, set the default.
            if (true == string.IsNullOrWhiteSpace(model.ModeCode))
            {
               model.ModeCode = "00-00-00-00";
            }

            IPublishedContent sitePage = Nodes.GetNodeByModeCode(model.ModeCode, false);

            IPublishedContent softwareItem = Software.GetSoftwareById(model.SoftwareId);

            string recipientsArray = "";

            if (softwareItem != null)
            {
               Extensions.Models.Aris.DownloadRequest downloadToSave = new Extensions.Models.Aris.DownloadRequest();

               downloadToSave.SoftwareId = model.SoftwareId;
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
               downloadToSave.HttpReferer = Request.UrlReferrer.AbsoluteUri;

               Extensions.Helpers.Aris.DownloadRequests.SaveDownloadRequest(downloadToSave);

               if (softwareItem.HasValue("recipients"))
               {
                  recipientsArray = softwareItem.GetPropertyValue<string>("recipients");

                  // Send email
                  Software.SendEmail(recipientsArray, softwareItem, downloadToSave);
               }

               TempData["downloadReady"] = true;

               string downloadUrl = "/research/software/download/";

               IPublishedContent downloadFileNode = Nodes.DownloadFile();

               if (downloadFileNode != null)
               {
                  downloadUrl = downloadFileNode.Url;
               }

               return new RedirectResult(downloadUrl + "?modeCode=" + model.ModeCode + "&softwareid=" + Server.UrlEncode(model.SoftwareId));
            }


            return CurrentUmbracoPage();
         }
      }
   }
}