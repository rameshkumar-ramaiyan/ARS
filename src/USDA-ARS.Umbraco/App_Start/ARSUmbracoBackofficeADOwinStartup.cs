using Microsoft.Owin;
using Owin;
using Umbraco.Web.Security.Identity;
using Umbraco.IdentityExtensions;
using USDA_ARS.Umbraco;
using Umbraco.Core.Models.Identity;
using ARS.ActiveDirectoryMembership;
using Umbraco.Core;
using Umbraco.Core.Security;

[assembly: OwinStartup("ARSUmbracoBackofficeADOwinStartup", typeof(ARSUmbracoBackofficeADOwinStartup))]

namespace USDA_ARS.Umbraco
{
    public class ARSUmbracoBackofficeADOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationContext = ApplicationContext.Current;

            app.ConfigureUserManagerForUmbracoBackOffice<BackOfficeUserManager, BackOfficeIdentityUser>(
                applicationContext,
                (options, context) =>
                {
                    var membershipProvider = MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider();
                    var userManager = BackOfficeUserManager.Create(options,
                        applicationContext.Services.UserService,
                        applicationContext.Services.ExternalLoginService,
                        membershipProvider);

                    //Set your own custom IBackOfficeUserPasswordChecker   
                    userManager.BackOfficeUserPasswordChecker = new ARSBackOfficeUserManager();

                    return userManager;
                });

            app.UseUmbracoBackOfficeCookieAuthentication(ApplicationContext.Current)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext.Current);
        }
    }
}