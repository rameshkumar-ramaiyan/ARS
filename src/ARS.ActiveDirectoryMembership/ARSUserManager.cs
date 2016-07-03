using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace ARS.ActiveDirectoryMembership
{
    public class ARSUserManager : BackOfficeUserManager
    {
        public ARSUserManager(IUserStore<BackOfficeIdentityUser, int> store)
        : base(store)
        {
        }

        public override Task<bool> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            // Validations coming here
            bool ret = LdapAuth(user.UserName, password);

            return Task.FromResult(ret);
        }

        private bool LdapAuth(string username, string password)
        {
            bool resp = false;
            try
            {
                string ldapRoot = ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
                var entry = new DirectoryEntry(ldapRoot, username, password);
                try
                {
                    var search = new DirectorySearcher(entry) { Filter = "(SAMAccountName=" + username + ")" };
                    search.PropertiesToLoad.Add("cn");
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        // Login was successful
                        resp = true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warn(typeof(ARSUserManager), "Problem with login: " + username + " | " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(typeof(ARSUserManager), "Problem with login: " + username + " | " + ex.ToString());
            }

            return resp;
        }

        public static ARSUserManager InitUserManager(ARSUserManager manager, MembershipProviderBase membershipProvider, IdentityFactoryOptions<BackOfficeUserManager> options)
        {
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<BackOfficeIdentityUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = membershipProvider.MinRequiredPasswordLength,
                RequireNonLetterOrDigit = membershipProvider.MinRequiredNonAlphanumericCharacters > 0,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false
            };

            //use a custom hasher based on our membership provider
            //THIS IS AN INTERNAL METHOD WHICH I PULL OUT INTO A CLASS BELOW
            //THIS SHOULD NOT BE NECESSARY IN v7.3.1
            manager.PasswordHasher = new MembershipPasswordHasher(membershipProvider);

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<BackOfficeIdentityUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = membershipProvider.MaxInvalidPasswordAttempts;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(30);

            //custom identity factory for creating the identity object for which we auth against in the back office
            manager.ClaimsIdentityFactory = new BackOfficeClaimsIdentityFactory();

            return manager;
        }
    }

    internal class MembershipPasswordHasher : IPasswordHasher
    {
        private readonly MembershipProviderBase _provider;

        public MembershipPasswordHasher(MembershipProviderBase provider)
        {
            _provider = provider;
        }

        public string HashPassword(string password)
        {
            return _provider.HashPasswordForStorage(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return _provider.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}