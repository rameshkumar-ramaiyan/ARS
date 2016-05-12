using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(USDA_ARS.ImportDocsClient.Startup))]
namespace USDA_ARS.ImportDocsClient
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
