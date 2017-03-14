using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodSpeak.Web.Models
{
    public class ApplicationUserProfile
    {
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserProfileId { get; set; }
        [Required]
        public string Code { get; set; }

        public int InviteBalance { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}