using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GodSpeak.Web.Models
{
    public class ShareAndroidGiftViewModel
    {
        [Required]
        public string Message { get; set; }
    }
}