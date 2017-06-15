using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GodSpeak.Web.Models
{
    public class EmailRequestApiObject
    {
        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }
    }


    public class GuidRequestApiObject
    {
        [Required]
        public Guid Guid { get; set; }
    }

    public class LoginApiObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class DeliveredMessageRequestApiObject
    {
        [Required]
        public string VerseCode { get; set; }

        [Required]
        public DateTime DateDelivered { get; set; }
    }

    public class RegisterReferralApiObject
    {
        [Required]
        [EmailAddress]
        public string ReferringEmailAddress { get; set; }
    }

    public class CountryCodeApiObject
    {
        public string Title { get; set; }

        public string Code { get; set; }
    }

    public class UpdateUserObject
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }

        public string PasswordConfirm { get; set; }
        

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public List<MessageCategorySettingApiObject> MessageCategorySettings { get; set; }

        [Required]
        public List<MessageDayOfWeekSettingApiObject> MessageDayOfWeekSettings { get; set; }


    }

    

    public class RegisterUserObject
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string PasswordConfirm { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string Platform { get; set; }

        public string ReferringInviteCode { get; set; }
        

       
    }

    public class AcceptedInviteObject
    {
        public string ImageUrl { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string ButtonTitle { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public string EmailAddress { get; set; }

        public int GiftsGiven { get; set; }

        public DateTime DateClaimed { get; set; }
    }

    public class ImpactApiObject
    {

        public static ImpactApiObject FromModel(ImpactDay model)
        {
            return new ImpactApiObject()
            {
                Date = model.Day,
                Points = model.Points.Select(ImpactPointApiObject.FromModel).ToList(),
                ScripturesDelivered = model.DeliveredMessages.Count
            };
        }

        public ICollection<ImpactPointApiObject> Points { get; set; }

        public DateTime Date { get; set; }

        public int ScripturesDelivered { get; set; }
    }

    public class MessageApiObject
    {
        public Guid Id { get; set; }
        public DateTime DateTimeToDisplay { get; set; }

        public VerseApiObject PreviousVerse { get; set; }

        public VerseApiObject Verse { get; set; }

        public VerseApiObject NextVerse { get; set; }
    }

    public class VerseApiObject
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public static VerseApiObject FromModel(BibleVerse verse)
        {
            return new VerseApiObject()
            {
                Title = verse.ShortCode,
                Text = verse.Text
            };
        }
    }

    public class ImpactPointApiObject
    {
        public static ImpactPointApiObject FromModel(ImpactDayGeoPoint model)
        {
            return new ImpactPointApiObject()
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };
        }

        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }

    public class UserApiObject
    {
       

        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PostalCode { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; }

        public string PhotoUrl { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public List<MessageCategorySettingApiObject> MessageCategorySettings {get; set; }

        public List<MessageDayOfWeekSettingApiObject> MessageDayOfWeekSettings { get; set; }

        public string ReferringEmailAddress { get; set; }

        public static UserApiObject FromModel(ApplicationUser user, ApplicationUserProfile profile, PostalCodeGeoLocation geoPoint)
        {
            var daysOfWeek = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var dayOfWeekSettings = profile.MessageDayOfWeekSettings.Select(MessageDayOfWeekSettingApiObject.FromModel).OrderBy(d => daysOfWeek.IndexOf(d.Title)).ToList();

            var categorySettings = profile.MessageCategorySettings.Select(MessageCategorySettingApiObject.FromModel).ToList();
            var top100Cat = categorySettings.Find(c => c.Title.Contains("100"));
            categorySettings.Insert(0,top100Cat);
            categorySettings.RemoveAt(categorySettings.LastIndexOf(top100Cat));
            var userObject = new UserApiObject()
            {
                FirstName = profile.FirstName,
                CountryCode = profile.CountryCode,
                Email = user.Email,
                LastName = profile.LastName,
                PostalCode = profile.PostalCode,
                PhotoUrl = profile.PhotoUrl,
                Token = profile.Token,
                MessageCategorySettings = categorySettings,
                MessageDayOfWeekSettings = dayOfWeekSettings,
                ReferringEmailAddress = profile.ReferringEmailAddress
               
                
            };
            if (geoPoint != null)
            {
                userObject.Latitude = geoPoint.Latitude;
                userObject.Longitude = geoPoint.Longitude;

            }
            return userObject;
        }

        
    }

    public class MessageDayOfWeekSettingApiObject
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }

        public string Title { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int NumOfMessages { get; set; }

        public static MessageDayOfWeekSettingApiObject FromModel(MessageDayOfWeekSetting model)
        {
            return new MessageDayOfWeekSettingApiObject()
            {
                Id = model.MessageDayOfWeekSettingId,
                Enabled = model.Enabled,
                Title = model.Title,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                NumOfMessages = model.NumOfMessages
            };
        }
    }

    public class MessageCategorySettingApiObject
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        
        public string Title { get; set; }

        public static MessageCategorySettingApiObject FromModel(MessageCategorySetting setting)
        {
            return new MessageCategorySettingApiObject()
            {
                Id = setting.MessageCategorySettingId,
                Title = setting.Category.Title,
                Enabled =  setting.Enabled
            };
        }
    }

    public class AppShareRequestObject
    {
        [Required]
        public List<string> ToEmailAddresses { get; set; }

        [Required]
        [EmailAddress]
        public string FromEmailAddress { get; set; }

        [Required]
        public string FromName { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Subject { get; set; }
    }

    public class ApiResponse
    {
        public string Title { get; set; }
        public string Message { get; set; }

    }

    public class ApiResponse<T>:ApiResponse where T : class
    {
        public T Payload { get; set; }

    }


}