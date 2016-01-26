using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public class NodeDownloadRequestsList
    {
        public string SoftwareTitle { get; set; }
        public List<Aris.DownloadRequest> DownloadRequestList { get; set; }
    }
}
