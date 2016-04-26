using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    public partial class EmailSignup
    {
        [Required]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a subscription")]
        public string ListName { get; set; }

        public string Action { get; set; }

        public int UmbracoId { get; set; }
    }
}
