using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;
using Umbraco.IdentityExtensions;
using USDA_ARS.Umbraco;
using Umbraco.Core.Models.Identity;
using ARS.ActiveDirectoryMembership;

[assembly: OwinStartup("ARSUmbracoADOwinStartup", typeof(ARSUmbracoADOwinStartup))]

namespace USDA_ARS.Umbraco
{
    public class ARSUmbracoADOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var applicationContext = ApplicationContext.Current;

            app.ConfigureUserManagerForUmbracoBackOffice<BackOfficeUserManager, BackOfficeIdentityUser>(
                ApplicationContext.Current,
                (options, context) =>
                {
                    var membershipProvider = MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider();

                    var store = new BackOfficeUserStore(
                                applicationContext.Services.UserService,
                                applicationContext.Services.ExternalLoginService,
                                membershipProvider);

                    return ARSUserManager.InitUserManager(new ARSUserManager(store), membershipProvider, options);
                });

            app.UseUmbracoBackOfficeCookieAuthentication(ApplicationContext.Current)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext.Current);
        }
    }
}