using System.Collections.Generic;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
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
                FirstName = "Benjamin",
                LastName = "Bishop",
                PostalCode = "63017",
                Code = "YgtFijl",
                CountryCode = "USA",
                InviteBalance = 3,
                MessageCategorySettings = new List<MessageCategorySetting>()
                {
                    new MessageCategorySetting()
                    {
                        Title = "Grief"
                    },
                    new MessageCategorySetting()
                    {
                        Title = "Love"
                    }
                }
                
                

            });

            CreateInvite(context, "AS5Invites", "PS5Invites", 2.99m, 5);
            CreateInvite(context, "AS15Invites", "PS15Invites", 3.99m, 15);
            CreateInvite(context, "AS25Invites", "PS25Invites", 4.99m, 25);
            CreateInvite(context, "AS50Invites", "PS50Invites", 6.99m, 50);
            CreateInvite(context, "AS100Invites", "PS100Invites", 10.99m, 100);
        }

        private void CreateInvite(ApplicationDbContext context, string appstoreSku, string playstoreSku, decimal cost, int count)
        {
            if (!context.InviteBundles.Any(b => b.AppStoreSku == appstoreSku))
                context.InviteBundles.Add(new InviteBundle() {AppStoreSku = appstoreSku, PlayStoreSku = playstoreSku, Cost = cost, NumberOfInvites = count});
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
                user.Profile.Token = profile.Token;
                user.Profile.MessageCategorySettings = profile.MessageCategorySettings;
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var authRepo = new AuthRepository(userManager, context);

            user.Profile.Token = authRepo.CalculateMd5Hash(user.Id + user.Email);

            context.Entry(user).State = EntityState.Modified;

            
            context.SaveChanges();
        }
    }
}
