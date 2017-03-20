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
         
            if(!context.BibleVerses.Any())
                LoadBibleVerses(context);

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
                },
                MessageDayOfWeekSettings = new List<MessageDayOfWeekSetting>()
                {
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Sunday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Monday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Tuesday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Wednesday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Thursday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Friday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    },
                    new MessageDayOfWeekSetting()
                    {
                        Title = "Saturday",
                        NumOfMessages = 3,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(17)
                    }

                }
                
                

            });

            CreateInvite(context, "AS5Invites", "PS5Invites", 2.99m, 5);
            CreateInvite(context, "AS15Invites", "PS15Invites", 3.99m, 15);
            CreateInvite(context, "AS25Invites", "PS25Invites", 4.99m, 25);
            CreateInvite(context, "AS50Invites", "PS50Invites", 6.99m, 50);
            CreateInvite(context, "AS100Invites", "PS100Invites", 10.99m, 100);
        }

        private void LoadBibleVerses(ApplicationDbContext context)
        {
            var parser = new BibleVerseParser();
            var binFolderPath= System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");


            
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/");

            foreach (var line in File.ReadLines(path + "NASBNAME.TXT"))
            {
                if(string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    var verse = parser.ParseLine(line);
                    if (!context.BibleVerses.Any(v => v.ShortCode == verse.ShortCode))
                    {
                        context.BibleVerses.Add(verse);
                        System.Diagnostics.Trace.WriteLine($"Added verse {verse.ShortCode}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error debugging line:\r" + line);
                }
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
                user.Profile.MessageCategorySettings = profile.MessageCategorySettings;
                user.Profile.MessageDayOfWeekSettings = profile.MessageDayOfWeekSettings;
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
