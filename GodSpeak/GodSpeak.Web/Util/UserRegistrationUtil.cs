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
        public async Task<List<string>> GetParentInviteCodes(string invite, ApplicationDbContext context)
        {
            var codes = new List<string>();
            var profile = await context.Profiles.FirstAsync(p => p.Code == invite);

            while (!string.IsNullOrEmpty(profile.ReferringCode))
            {
                var referringCode = profile.ReferringCode;
                codes.Add(referringCode);
                profile = await context.Profiles.FirstAsync(p => p.Code == referringCode);
            }

            return codes;
        }

        public List<MessageCategorySetting> GenerateDefaultMessageCategorySettings(ApplicationDbContext context)
        {
            return
                context.MessageCategories.ToList().Select(category => new MessageCategorySetting() { Category = category, Enabled = true })
                    .ToList();
        }

        public ICollection<MessageDayOfWeekSetting> GenerateDefaultDayOfWeekSettingsForUser(ApplicationDbContext context)
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}