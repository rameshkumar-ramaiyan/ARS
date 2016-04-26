using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace USDA_ARS.ImportLocationMap.Models
{
    [TableName("MapCoordinates")]
    public class MapCoordinate
    {
        [Column("modecodeconc")]
        public string ModeCodeConc { get; set; }

        [Column("modecode_1")]
        public string ModeCode1 { get; set; }

        [Column("modecode_2")]
        public string ModeCode2 { get; set; }

        [Column("modecode_3")]
        public string ModeCode3 { get; set; }

        [Column("modecode_4")]
        public string ModeCode4 { get; set; }

        [Column("modecode_1_desc")]
        public string ModeCode1Desc { get; set; }

        [Column("modecode_2_desc")]
        public string ModeCode2Desc { get; set; }

        [Column("coordinates")]
        public string Coordinates { get; set; }

        [Column("Latitude")]
        public decimal Latitude { get; set; }

        [Column("Longitude")]
        public decimal Longitude { get; set; }
    }
}
