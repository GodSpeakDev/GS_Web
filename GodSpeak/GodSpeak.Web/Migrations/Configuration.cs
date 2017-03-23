using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;
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
         
            if (!context.MessageCategories.Any())
                LoadMessageCategories(context);

            CreateUser(context, "ben@rendr.io", "J0hn_galt");

            var registerUtil = new UserRegistrationUtil(context);

            var ben = new ApplicationUserProfile()
            {
                FirstName = "Benjamin",
                LastName = "Bishop",
                PostalCode = "63017",
                Code = registerUtil.GenerateInviteCode(),
                ReferringCode = "godspeak",
                CountryCode = "US",
                InviteBalance = 3,
                MessageCategorySettings = registerUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = registerUtil.GenerateDefaultDayOfWeekSettingsForUser()

            };
            AddOrUpdateProfileToUser(context, "ben@rendr.io", ben);

//            CreateUser(context, "brett@venadotech.com", "v3nad0");
//            var brett = new ApplicationUserProfile()
//            {
//                FirstName = "Brett",
//                LastName = "Williams",
//                PostalCode = "74055",
//                Code = registerUtil.GenerateInviteCode(),
//                ReferringCode = ben.Code,
//                CountryCode = "US",
//                InviteBalance = 3,
//                MessageCategorySettings = registerUtil.GenerateDefaultMessageCategorySettings(),
//                MessageDayOfWeekSettings = registerUtil.GenerateDefaultDayOfWeekSettingsForUser()
//
//            };
//            AddOrUpdateProfileToUser(context, "brett@venadotech.com", brett);
//
//            var impactRepo = new ImpactRepository(context, new UserRegistrationUtil(context), new InMemoryDataRepository());
//            impactRepo.RecordImpact(DateTime.Now, ben.PostalCode, ben.CountryCode, ben.ReferringCode).Wait();
//
//            impactRepo.RecordImpact(DateTime.Now, brett.PostalCode, brett.CountryCode, brett.ReferringCode).Wait();

            CreateInvite(context, "AS5Invites", "PS5Invites", 2.99m, 5);
            CreateInvite(context, "AS15Invites", "PS15Invites", 3.99m, 15);
            CreateInvite(context, "AS25Invites", "PS25Invites", 4.99m, 25);
            CreateInvite(context, "AS50Invites", "PS50Invites", 6.99m, 50);
            CreateInvite(context, "AS100Invites", "PS100Invites", 10.99m, 100);
        }

        

       

        private void LoadMessageCategories(ApplicationDbContext context)
        {
             var titles = new List<string>()
             {
                 "Love",
                 "Faith",
                 "Peace",
                 "Hope",
                 "Marriage",
                 "Joy",
                 "Prayer",
                 "Strength",
                 "Grace",
                 "Children",
                 "Forgiveness",
                 "Healing",
                 "Holy Spirit",
                 "Salvation",
                 "Fear",
                 "Top 100 Most Searched/Read Bible Verses [2016]",
                 "Wisdom from Proverbs",
                 "What Jesus Said"
             };
            foreach (var title in titles)
            {
                context.MessageCategories.Add(new MessageCategory() {Title = title});
            }
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
//                user.Profile.MessageCategorySettings = profile.MessageCategorySettings;
//                user.Profile.MessageDayOfWeekSettings = profile.MessageDayOfWeekSettings;
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
