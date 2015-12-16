using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("NewModecodes")]
    public class ModeCodeNew
    {
        [Column("OldModecode")]
        public string ModecodeOld { get; set; }

        [Column("NewModecode")]
        public string ModecodeNew { get; set; }
    }
}
