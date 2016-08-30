using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
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
         Task<BackOfficeUserPasswordCheckerResult> result = null;
         bool useActiveDirectlyOnly = false;

         if (ConfigurationManager.AppSettings["ADSetting:UserActiveDirectoryOnly"] != null && ConfigurationManager.AppSettings.Get("ADSetting:UserActiveDirectoryOnly").ToLower() == "true")
         {
            useActiveDirectlyOnly = true;
         }

         if (true == useActiveDirectlyOnly)
         {
            if (true == LdapAuth(user.UserName, password))
            {
               result = Task.FromResult(BackOfficeUserPasswordCheckerResult.ValidCredentials);
            }
         }
         else
         {
            result = LdapAuth(user.UserName, password)
                   ? Task.FromResult(BackOfficeUserPasswordCheckerResult.ValidCredentials)
                   : Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);
         }

         return result;
      }

      private bool LdapAuth(string username, string password)
      {
         LogHelper.Info(typeof(ARSBackOfficeUserManager), "Attempting login via LDAP: " + username);

         bool resp = false;
         try
         {
            List<string> ldapArray = new List<string>();

            if (ConfigurationManager.ConnectionStrings["ADConnectionString"] != null &&
                  false == string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString))
            {
               ldapArray = ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString.Split(';').ToList();

               if (ldapArray != null && ldapArray.Any())
               {
                  foreach (string ldapConnection in ldapArray)
                  {
                     if (resp == false)
                     {
                        LogHelper.Info(typeof(ARSBackOfficeUserManager), "Using LDAP: " + ldapConnection);

                        var entry = new DirectoryEntry(ldapConnection, username, password);
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
                           LogHelper.Warn(typeof(ARSBackOfficeUserManager), "Problem with login: " + username);
                        }
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            LogHelper.Warn(typeof(ARSBackOfficeUserManager), "Problem with ldap. Username: " + username);
         }

         return resp;
      }
   }
}
