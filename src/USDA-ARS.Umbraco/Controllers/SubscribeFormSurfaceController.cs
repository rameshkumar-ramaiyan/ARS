using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Controllers
{
   public class SubscribeFormSurfaceController : SurfaceController
   {
      // GET: EmailSignup
      public ActionResult FormArsNews()
      {
         EmailSignup emailSignup = new EmailSignup();

         emailSignup.UmbracoId = UmbracoContext.PageId ?? 0;

         return PartialView("SubscribeFormArsNews", emailSignup);
      }

      public ActionResult FormFoodNutrition()
      {
         EmailSignup emailSignup = new EmailSignup();

         emailSignup.UmbracoId = UmbracoContext.PageId ?? 0;

         return PartialView("SubscribeFormFoodNutrition", emailSignup);
      }

      public ActionResult FormHealthyAnimals()
      {
         EmailSignup emailSignup = new EmailSignup();

         emailSignup.ListName = "healthyanimals";

         emailSignup.UmbracoId = UmbracoContext.PageId ?? 0;

         return PartialView("SubscribeFormHealthyAnimals", emailSignup);
      }

						public ActionResult FormArsJobs()
						{
									EmailSignup emailSignup = new EmailSignup();

									emailSignup.UmbracoId = UmbracoContext.PageId ?? 0;

									return PartialView("SubscribeToArsJobs", emailSignup);
						}

						[NotChildAction]
      [HttpPost]
      public ActionResult HandleFormSubmit(EmailSignup model)
      {
         if (false == ModelState.IsValid)
         {
            return CurrentUmbracoPage();
         }
         else
         {
            IPublishedContent formNode = Umbraco.TypedContent(model.UmbracoId);

            if (formNode != null)
            {
               string emailTo = formNode.GetPropertyValue<string>("emailToSubscribe");
               string emailFrom = model.Email;

               if (model.Action != null && model.Action.ToLower().IndexOf("unsubscribe") >= 0)
               {
                  emailTo = formNode.GetPropertyValue<string>("emailToUnsubscribe");
               }

               emailTo = emailTo.Replace("{{LIST_NAME}}", model.ListName.ToLower());

               // TESTING
               //emailTo = "john.skufca@axial.agency";

               SubscriptionForm.SendEmail(emailFrom, emailTo, model.Action + " me to " + model.ListName, "\r\n" + model.Action.ToLower() + "\r\n");

               return Redirect("/elists/");
            }
            else
            {
               return CurrentUmbracoPage();
            }
         }
      }
   }
}