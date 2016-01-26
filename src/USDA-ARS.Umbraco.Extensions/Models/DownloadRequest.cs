using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public partial class DownloadRequest
    {
        public string ModeCode { get; set; }

        public int downreqid { get; set; }
        public string SoftwareId { get; set; }

        [StringLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [StringLength(100)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [StringLength(10)]
        [DisplayName("Middle")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public string Affiliation { get; set; }

        public string Purpose { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

        public Nullable<System.DateTime> TimeStamp { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Reference { get; set; }
    }
}
