using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace USDA_ARS.LocationsWebApp.Models
{
    public class News
    {
        [Column("NewsID")]
        public int NewsID { get; set; }

        [Column("SubjectField")]
        public string SubjectField { get; set; }

        [Column("ISFileName")]
        public string ISFileName { get; set; }

        [Column("DateField")]
        public DateTime DateField { get; set; }

        [Column("published")]
        public string Published { get; set; }
    }
}