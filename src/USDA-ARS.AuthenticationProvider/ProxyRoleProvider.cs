using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web;

namespace ARS.AuthenticationProvider
{
    public class ProxyRoleProvider : RoleProvider
    {

        private RoleProvider _provider;
        private const string ADRoleProviderName = "ProxyRoleProvider";
        private const string UmbracoRoleProviderName = "UmbracoRoleProvider";

        public RoleProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = Roles.Providers[UmbracoRoleProviderName];
                }
                return _provider;
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            _provider = Roles.Providers[ADRoleProviderName];
            _provider.AddUsersToRoles(usernames, roleNames);
        }

        public override string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public override void CreateRole(string roleName)
        {
            _provider.CreateRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return _provider.DeleteRole(roleName, throwOnPopulatedRole);
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return _provider.FindUsersInRole(roleName, usernameToMatch);
        }
        public override string[] GetAllRoles()
        {
            return _provider.GetAllRoles();
        }

        public override string[] GetRolesForUser(string username)
        {
            return _provider.GetRolesForUser(username);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return _provider.GetUsersInRole(roleName);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return _provider.IsUserInRole(username, roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            _provider.RemoveUsersFromRoles(usernames, roleNames);
        }

        public override bool RoleExists(string roleName)
        {
            return _provider.RoleExists(roleName);
        }
    }
}
