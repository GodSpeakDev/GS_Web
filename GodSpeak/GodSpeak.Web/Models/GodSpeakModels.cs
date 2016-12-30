using System;

namespace GodSpeak.Web.Models
{
    public class GodSpeakModels
    {
        public class ApplicationUserInvite
        {
            public Guid InviteId { get; set; }

            public string Code { get; set; }

            public virtual ApplicationUser User { get; set; }
        }
    }
}