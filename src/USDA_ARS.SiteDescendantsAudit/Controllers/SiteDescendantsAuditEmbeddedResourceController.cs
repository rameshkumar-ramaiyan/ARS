using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Core.Logging;

namespace USDA_ARS.SiteDescendantsAudit.Controllers
{
    // Get embedded resources files (.html, .js, .css, ...) 
    public class SiteDescendantsAuditEmbeddedResourceController : Controller
    {
        public FileStreamResult GetResource(string resource)
        {
            try
            {
                // Get this assembly
                Assembly assembly = typeof(SiteDescendantsAuditEmbeddedResourceController).Assembly;

                var manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                var resourceName = manifestResourceNames.ToList().FirstOrDefault(f => f.EndsWith(resource));
                //LogHelper.Info(typeof(EmbeddedResourceController), string.Format("Getting the resource: {0}", resourceName));

                if (!String.IsNullOrEmpty(resourceName))
                {
                    return new FileStreamResult(assembly.GetManifestResourceStream(resourceName), this.GetMimeType(resourceName));
                }
                else
                {
                    LogHelper.Warn(typeof(SiteDescendantsAuditEmbeddedResourceController), string.Format("Couldn't get the resource: {0}", resource));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SiteDescendantsAuditEmbeddedResourceController), string.Format("Couldn't get the resource: {0}", resource), ex);
            }

            return null;
        }

        private string GetMimeType(string resource)
        {
            switch (Path.GetExtension(resource))
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "text";
            }
        }

    }
}
