using System;
using System.Collections.Generic;
using System.Linq;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Util
{
    public class UserRegistrationUtil
    {
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

    }
}