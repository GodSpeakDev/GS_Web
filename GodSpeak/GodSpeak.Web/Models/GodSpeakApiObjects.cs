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

    public class LoginApiObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserApiObject
    {
        public Guid UserId { get; set; }

        public int InviteBalance { get; set; }

        public string InviteCode { get; set; }

        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PostalCode { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; }

        public string PhotoUrl { get; set; }

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
                Token = user.Id,
                UserId = new Guid(user.Id)
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