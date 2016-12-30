using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GodSpeak.Web.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<GodSpeakModels.ApplicationUserInvite> Invites { get; set; }
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}