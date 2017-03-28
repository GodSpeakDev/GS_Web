using System.Data.Entity;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GodSpeak.Web.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUserProfile> Profiles { get; set; }

        public DbSet<InviteBundle> InviteBundles { get; set; }

        public DbSet<Message> Messages{ get; set; }

        public DbSet<MessageCategory> MessageCategories { get; set; }

        public DbSet<ImpactDay> ImpactDays { get; set; }

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