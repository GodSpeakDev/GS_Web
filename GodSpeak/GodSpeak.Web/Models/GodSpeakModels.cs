﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodSpeak.Web.Models
{

    public class ApplicationUserInvite
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ApplicationUserInviteId { get; set; }
        [Required]
        public string Code { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}