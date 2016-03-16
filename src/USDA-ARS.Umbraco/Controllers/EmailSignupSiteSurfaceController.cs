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
    public class EmailSignupSiteSurfaceController : SurfaceController
    {
        // GET: EmailSignup
        public ActionResult Index()
        {
            EmailSignup emailSignup = new EmailSignup();

            emailSignup.ListName = "arsnews";
            emailSignup.Action = "Subscribe";


            return PartialView("EmailSignupSite", emailSignup);

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


                return Redirect("/elists/");
            }
        }
    }
}