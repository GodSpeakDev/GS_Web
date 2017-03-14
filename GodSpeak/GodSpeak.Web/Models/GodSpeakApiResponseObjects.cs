using System;

namespace GodSpeak.Web.Models
{
    public class UserResponseObject
    {
        public Guid UserId { get; set; }   

        public int InviteBalance { get; set; }

        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PostalCode { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; }

        public string PhotoUrl { get; set; }

        public static UserResponseObject FromModel(ApplicationUser user)
        {
            return new UserResponseObject()
            {
                FirstName = user.Profile.FirstName,
                CountryCode = user.Profile.CountryCode,
                Email = user.Email,
                InviteBalance = user.Profile.InviteBalance,
                LastName = user.Profile.LastName,
                PostalCode = user.Profile.PostalCode,
                PhotoUrl = "",
                Token = user.Id,
                UserId = new Guid(user.Id)
            };
        }
    }
}