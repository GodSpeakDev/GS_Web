using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Util
{
    public class UserRegistrationUtil
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRegistrationUtil(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<string>> GetParentEmailAddresses(string emailAddress)
        {
            var addresses = new List<string>() {"impact@godspeak.com"};

            if (string.IsNullOrEmpty(emailAddress))
                return addresses;

            var firstUserId = (await _dbContext.Users.FirstAsync(u => u.Email == emailAddress)).Id;

            var profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == firstUserId);

            while (!string.IsNullOrEmpty(profile.ReferringEmailAddress))
            {
                var referringEmailAddress = profile.ReferringEmailAddress;
                addresses.Add(referringEmailAddress);
                if (string.IsNullOrEmpty(referringEmailAddress) || referringEmailAddress == "impact@godspeak.com")
                    break;
                var userId = (await _dbContext.Users.FirstAsync(u => u.Email == profile.ReferringEmailAddress)).Id;
                profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == userId);
            }

            return addresses.Distinct().ToList();
        }

        public async Task<List<string>> GetParentInviteCodes(string inviteCode)
        {
            var codes = new List<string>() { "godspeak" };

            if (string.IsNullOrEmpty(inviteCode))
                return codes;

  

            var profile = await _dbContext.Profiles.FirstAsync(p => p.Code == inviteCode);

            while (!string.IsNullOrEmpty(profile.ReferringEmailAddress))
            {
                var referringEmailAddress = profile.ReferringEmailAddress;
                codes.Add(profile.Code);
                if (string.IsNullOrEmpty(referringEmailAddress) || referringEmailAddress == "impact@godspeak.com")
                    break;
                var userId = (await _dbContext.Users.FirstAsync(u => u.Email == profile.ReferringEmailAddress)).Id;
                profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == userId);
            }

            return codes.Distinct().ToList();
        }

        public List<MessageCategorySetting> GenerateDefaultMessageCategorySettings()
        {
            return
                _dbContext.MessageCategories.ToList().Select(category => new MessageCategorySetting() { Category = category, Enabled = (category.Title.Contains("100")) })
                    .ToList();
        }

        public ICollection<MessageDayOfWeekSetting> GenerateDefaultDayOfWeekSettingsForUser()
        {
            var daysOfWeek = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return daysOfWeek.Select(d => new MessageDayOfWeekSetting()
            {
                Title = d,
                Enabled = true,
                NumOfMessages = 3,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(17)
            }).ToList();
        }
        private readonly Random _random = new Random();
        public string GenerateInviteCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}