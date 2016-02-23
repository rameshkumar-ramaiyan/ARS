using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class ExternalReferer
    {
        public static string GetExternalUrl(Uri referer)
        {
            string externalUrl = null;

            if (referer != null && false == string.IsNullOrEmpty(referer.AbsoluteUri))
            {
                if (HttpContext.Current.Request.Url.DnsSafeHost != referer.DnsSafeHost)
                {
                    externalUrl = referer.AbsoluteUri;
                }
            }

            return externalUrl;
        }
    }
}
