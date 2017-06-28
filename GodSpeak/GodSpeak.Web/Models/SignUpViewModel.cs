using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GodSpeak.Web.Models
{
    public class SignUpViewModel
    {
        [Required]
        [Display(Name = "Your Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string InviteCode { get; set; }

        [Required]
        [Display(Name = "Phone Type")]
        public PhonePlatforms Platform { get; set; }
    }

    public enum PhonePlatforms
    {
        iPhone,
        Android
    }
}