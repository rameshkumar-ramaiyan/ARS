using log4net;
using System;
using System.Configuration;
using System.Configuration.Provider;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Security.Providers;

namespace USDA_ARS.AuthenticationProvider
{
    public class ProxyMembershipProvider : UmbracoMembershipProvider<IMembershipUserService, IUser>
    {
        private MembershipProvider _provider;
        private readonly string ADProviderName;
        private readonly string UmbracoProviderName;
        private string _defaultMemberTypeAlias = "writer";
        private volatile bool _hasDefaultMember = false;
        private static readonly object Locker = new object();

        public MembershipProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = Membership.Providers[UmbracoProviderName];
                }
                return _provider;
            }
        }

        public ProxyMembershipProvider()
            : this(ApplicationContext.Current.Services.UserService)
        {
            ADProviderName = ConfigurationManager.AppSettings["ADProviderName"];
            UmbracoProviderName = ConfigurationManager.AppSettings["UmbracoProviderName"];
        }

        public ProxyMembershipProvider(IMembershipMemberService<IUser> memberService)
            : base(memberService)
        {
        }

        protected override MembershipUser ConvertToMembershipUser(IUser entity)
        {
            var membershipMember = new MembershipUser(Provider.Name, entity.Name, entity.ProviderUserKey, entity.Email, entity.PasswordQuestion,
                    entity.Comments, entity.IsApproved, entity.IsLockedOut, entity.CreateDate.ToUniversalTime(), entity.LastLoginDate.ToUniversalTime(),
                    entity.LastLoginDate.ToUniversalTime(), entity.LastPasswordChangeDate.ToUniversalTime(), entity.LastLockoutDate.ToUniversalTime());
            return membershipMember;
        }

        public override string ProviderName
        {
            get { return UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider; }
        }

        public override bool ValidateUser(string username, string password)
        {
            ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            string[] LDAPNames = ConfigurationManager.AppSettings["LDAPName"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] LDAPContainers = ConfigurationManager.AppSettings["LDAPContainer"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string LDAPName = string.Empty;
            string LDAPContainer = string.Empty;
            bool result = false;
            try
            {
                for (int i = 0; i < LDAPNames.Length; i++)
                {
                    LDAPName = LDAPNames[i];
                    LDAPContainer = LDAPContainers[i];
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain, LDAPName, LDAPContainer, username, password);
                    if (ctx.ValidateCredentials(username, password))
                    {
                        log.Debug(string.Format("Successfully logged in with Active Directory with LDAP name: {0} and LDAPContainer: {1}", LDAPName, LDAPContainer));
                        result = true;
                        break;
                    }
                }
                if (!result)
                {
                    _provider = Membership.Providers[UmbracoProviderName];
                    if (_provider.ValidateUser(username, password))
                    {
                        log.Debug("Successfully logged in with Umbraco Membership");
                        result = true;
                    }
                    else
                    {
                        log.Debug("Some how AD authentication failed and umbraco authentication failed also");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {

                log.Error(string.Format("An error has occurred with logging in with Active Directory with LDAP name: {0} and LDAPContainer: {1}", LDAPName, LDAPContainer), e);
                _provider = null;
                result = false;
            }
            return result;
        }

        public override string DefaultMemberTypeAlias
        {
            get
            {
                if (_hasDefaultMember == false)
                {
                    lock (Locker)
                    {
                        if (_hasDefaultMember == false)
                        {
                            _defaultMemberTypeAlias = MemberService.GetDefaultMemberType();
                            if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                            {
                                throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                            }
                            _hasDefaultMember = true;
                        }
                    }
                }
                return _defaultMemberTypeAlias;
            }
        }
    }
}
