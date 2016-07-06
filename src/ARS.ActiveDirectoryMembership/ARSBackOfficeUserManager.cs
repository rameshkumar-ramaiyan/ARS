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
            LogHelper.Info(typeof(ARSBackOfficeUserManager), "Attempting login via LDAP: " + username);

            bool resp = false;
            try
            {
                string ldapRoot = ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;

                LogHelper.Info(typeof(ARSBackOfficeUserManager), "Using LDAP: " + ldapRoot);

                var entry = new DirectoryEntry(ldapRoot, username, password);
                try
                {
                    LogHelper.Info(typeof(ARSBackOfficeUserManager), "Searching username: " + username);

                    var search = new DirectorySearcher(entry) { Filter = "(SAMAccountName=" + username + ")" };
                    search.PropertiesToLoad.Add("cn");
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        LogHelper.Info(typeof(ARSBackOfficeUserManager), "Username authenticated: " + username);
                        // Login was successful
                        resp = true;
                    }
                    else
                    {
                        LogHelper.Warn(typeof(ARSBackOfficeUserManager), "Username NOT authenticated: " + username);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warn(typeof(ARSBackOfficeUserManager), "Problem with login: " + username + " | " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(typeof(ARSBackOfficeUserManager), "Problem with login: " + username + " | " + ex.ToString());
            }

            return resp;
        }
    }
}
