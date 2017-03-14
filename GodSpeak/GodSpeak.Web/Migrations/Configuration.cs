using GodSpeak.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GodSpeak.Web.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
        
        protected override void Seed(GodSpeak.Web.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            CreateUser(context, "ben@rendr.io", "J0hn_galt");
            AddOrUpdateProfileToUser(context, "ben@rendr.io", new ApplicationUserProfile()
            {
                FirstName = "Ben",
                LastName = "Bishop",
                PostalCode = "63017",
                Code = "YgtFijl",
                CountryCode = "USA",
                InviteBalance = 3


            });
        }

        private static void CreateUser(ApplicationDbContext context, string email, string password)
        {
            if (!(context.Users.Any(u => u.UserName == email)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser {UserName = email, Email = email};

                userManager.Create(userToInsert, password);
            }
        }

        private static void AddOrUpdateProfileToUser(ApplicationDbContext context, string email,
            ApplicationUserProfile profile)
        {
            var user = context.Users.First(u => u.Email == email);
            if (user.Profile == null)
            {
                user.Profile = profile;
                context.Profiles.Add(profile);
            }
            else
            {
                user.Profile.FirstName = profile.FirstName;
                user.Profile.LastName = profile.LastName;
                user.Profile.Code = profile.Code;
                user.Profile.CountryCode = profile.CountryCode;
                user.Profile.InviteBalance = profile.InviteBalance;
                user.Profile.PostalCode = profile.PostalCode;
            }
            context.Entry(user).State = EntityState.Modified;

            
            context.SaveChanges();
        }
    }
}
