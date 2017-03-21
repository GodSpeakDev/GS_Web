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

    public class CountryCodeApiObject
    {
        public string Title { get; set; }

        public string Code { get; set; }
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

        [Required]
        public string InviteCode { get; set; }
    }

    public class UserApiObject
    {
       

        public int InviteBalance { get; set; }

        public string InviteCode { get; set; }

        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PostalCode { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; }

        public string PhotoUrl { get; set; }

        public List<MessageCategorySettingApiObject> MessageCategorySettings {get; set; }

        public List<MessageDayOfWeekSettingApiObject> MessageDayOfWeekSettings { get; set; }

        public static UserApiObject FromModel(ApplicationUser user)
        {
            return new UserApiObject()
            {
                FirstName = user.Profile.FirstName,
                CountryCode = user.Profile.CountryCode,
                Email = user.Email,
                InviteBalance = user.Profile.InviteBalance,
                InviteCode = user.Profile.Code,
                LastName = user.Profile.LastName,
                PostalCode = user.Profile.PostalCode,
                PhotoUrl = "",
                Token = user.Profile.Token,
                MessageCategorySettings = user.Profile.MessageCategorySettings.Select(MessageCategorySettingApiObject.FromModel).ToList(),
                MessageDayOfWeekSettings = user.Profile.MessageDayOfWeekSettings.Select(MessageDayOfWeekSettingApiObject.FromModel).ToList()
                
            };
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