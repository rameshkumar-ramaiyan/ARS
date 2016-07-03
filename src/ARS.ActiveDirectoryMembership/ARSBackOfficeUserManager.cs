using System;
using System.Configuration;
using System.DirectoryServices;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace ARS.ActiveDirectoryMembership
{
    public class ARSBackOfficeUserManager : IBackOfficeUserPasswordChecker
    {
        public Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            var result = LdapAuth(user.UserName, password)
                ? Task.FromResult(BackOfficeUserPasswordCheckerResult.ValidCredentials)
                : Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);

            return result;
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
    }
}
