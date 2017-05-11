using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace GodSpeak.Web.Controllers
{
    [Route("api/seed/{action}")]
    public class SeedController : ApiControllerBase
    {
        private readonly IImpactRepository _impactRepo;
        private readonly ApplicationDbContext _dbContext;

        private string AppDataPath
        {
            get
            {
                var binFolderPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
                binFolderPath += "App_Data/";
                if (Directory.Exists(binFolderPath))
                    return binFolderPath;

                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/");
                return path;
            }
        }

        public SeedController(IImpactRepository impactRepo, IAuthRepository authoRepo, ApplicationDbContext dbContext):base(authoRepo)
        {
            _impactRepo = impactRepo;
            _dbContext = dbContext;
        }

        

       

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("fixCategories")]
        public async Task<HttpResponseMessage> FixCategories()
        {
            var badNames = new List<string>()
            {
                "Top 100 Searched Verses",
                "Top 100 Searched Verse",
                "Top 100",
                "Top100"
            };
            
            var correctCat = _dbContext.MessageCategories.First(m => m.Title.Contains("Top 100 Most Searched"));
            correctCat.Title = "Top 100 Most Searched";
            var profiles = _dbContext.Profiles.ToList();
            
            foreach (var badName in badNames)
            {
                if(!_dbContext.MessageCategories.Any(c => c.Title == badName))
                    continue;
                
                var cat = _dbContext.MessageCategories.First(c => c.Title == badName);
                foreach (var catMessage in cat.Messages)
                {
                    correctCat.Messages.Add(catMessage);
                }
                
                foreach (var profile in profiles)
                {
                    profile.MessageCategorySettings.Remove(profile.MessageCategorySettings.First(c => c.Category == cat));
                    _dbContext.Entry(profile).State = EntityState.Modified;
                }
                _dbContext.MessageCategories.Remove(cat);
            }
            _dbContext.Entry(correctCat).State = EntityState.Modified;
            

            _dbContext.SaveChanges();

            return CreateResponse(HttpStatusCode.OK, "Categories Cleaned Up", "");
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("populate")]
        public async Task<HttpResponseMessage> Populate()
        {
            if (_dbContext.Users.Any(u => u.Email == "ben@rendr.io"))
                return CreateResponse(HttpStatusCode.OK, "Database Already Seeded",
                    "The database has already been seeded");
            await Seed();
            return CreateResponse(HttpStatusCode.OK, "Database Seeded", "Database was successfully seeded");
        }
        
        protected async Task Seed()
        {

            if (!_dbContext.MessageCategories.Any())
                LoadMessages(_dbContext);

            CreateUser(_dbContext, "ben@rendr.io", "J0hn_galt");

            var registerUtil = new UserRegistrationUtil(_dbContext);

            var ben = new ApplicationUserProfile()
            {
                FirstName = "Benjamin",
                LastName = "Bishop",
                PostalCode = "63017",
                Code = registerUtil.GenerateInviteCode(),
                ReferringEmailAddress = "godspeak",
                CountryCode = "US",
                MessageCategorySettings = registerUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = registerUtil.GenerateDefaultDayOfWeekSettingsForUser()

            };
            AddOrUpdateProfileToUser(_dbContext, "ben@rendr.io", ben);

            CreateUser(_dbContext, "brett@venadotech.com", "v3nad0");
            var brett = new ApplicationUserProfile()
            {
                FirstName = "Brett",
                LastName = "Williams",
                PostalCode = "74055",
                Code = registerUtil.GenerateInviteCode(),
                ReferringEmailAddress = ben.Code,
                CountryCode = "US",
                MessageCategorySettings = registerUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = registerUtil.GenerateDefaultDayOfWeekSettingsForUser()

            };
            AddOrUpdateProfileToUser(_dbContext, "brett@venadotech.com", brett);

            CreateUser(_dbContext, "donations@godspeakapp.com", "v3nad0");
            var donations = new ApplicationUserProfile()
            {
                FirstName = "Anonymous",
                LastName = "Donor",
                PostalCode = "74055",
                Code = registerUtil.GenerateInviteCode(),
                ReferringEmailAddress = ben.Code,
                CountryCode = "US",
                MessageCategorySettings = registerUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = registerUtil.GenerateDefaultDayOfWeekSettingsForUser()

            };
            AddOrUpdateProfileToUser(_dbContext, "donations@godspeakapp.com", donations);




            await _impactRepo.RecordImpact(DateTime.Now.AddDays(-1), ben.PostalCode, ben.CountryCode, ben.Code);

            await _impactRepo.RecordImpact(DateTime.Now, brett.PostalCode, brett.CountryCode, brett.Code);
            
        }





        private void LoadMessages(ApplicationDbContext context)
        {

            var categories = new List<string>();
            var lines = File.ReadLines(AppDataPath + "GodSpeakVersesbyCategory.txt").ToList();
            foreach (var line in lines)
                if (!string.IsNullOrEmpty(line) && !line.Contains(":"))
                    categories.AddRange(line.Split(',').Select(s => s.Trim()).ToList());

            categories.Distinct().ForEach(c => context.MessageCategories.Add(new MessageCategory() { Title = c }));
            context.SaveChanges();
            for (var i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                if (line.Contains(":"))
                {
                    var catTitles = lines[i + 1].Split(',').Select(s => s.Trim()).ToList();
                    var message = new Message()
                    {
                        VerseCode = line.Trim()
                    };

                    context.Messages.Add(message);
                    context.SaveChanges();
                    var matchingCats = context.MessageCategories.Where(mc => catTitles.Contains(mc.Title)).ToList();
                    //                    var messageFromContext = context.Messages.First(m => m.VerseCode == message.VerseCode);
                    foreach (var messageCategory in matchingCats)
                    {
                        message.Categories.Add(messageCategory);
                        context.SaveChanges();
                    }
                    i += 1;
                }
            }
            context.SaveChanges();
        }
        

        private static void CreateUser(ApplicationDbContext context, string email, string password)
        {
            if (!(context.Users.Any(u => u.UserName == email)))
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser { UserName = email, Email = email };

                userManager.Create(userToInsert, password);
            }
        }

        private static void AddOrUpdateProfileToUser(ApplicationDbContext context, string email,
            ApplicationUserProfile profile)
        {
            var user = context.Users.First(u => u.Email == email);
            profile.UserId = user.Id;
            var profileToUpdate = context.Profiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profileToUpdate == null)
            {

                context.Profiles.Add(profile);
                context.SaveChanges();
                profileToUpdate = profile;
            }
            else
            {
                profileToUpdate.FirstName = profile.FirstName;
                profileToUpdate.LastName = profile.LastName;
                profileToUpdate.Code = profile.Code;
                profileToUpdate.CountryCode = profile.CountryCode;
                profileToUpdate.PostalCode = profile.PostalCode;
                
                //                user.Profile.MessageCategorySettings = profile.MessageCategorySettings;
                //                user.Profile.MessageDayOfWeekSettings = profile.MessageDayOfWeekSettings;
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var authRepo = new AuthRepository(userManager, context);

            
            //
            //            context.Entry(user).State = EntityState.Modified;
            //
            //            
            //                context.SaveChanges();

        }
    }
}
