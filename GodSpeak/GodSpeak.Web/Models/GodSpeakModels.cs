using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace GodSpeak.Web.Models
{
    public class ApplicationUserProfile
    {
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserProfileId { get; set; }
        [Required]
        public string Code { get; set; }

        
        public string ReferringCode { get; set; }

        public int InviteBalance { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string Token { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<MessageCategorySetting> MessageCategorySettings { get; set; }

       
        public virtual ICollection<MessageDayOfWeekSetting> MessageDayOfWeekSettings { get; set; }
    }

    public class MessageCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageCategoryId { get; set; }

        [Required]
        public string Title { get; set; }


    }

    public class MessageCategorySetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageCategorySettingId { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }
        
        public string ApplicationUserProfileRefId { get; set; }

        [ForeignKey("ApplicationUserProfileRefId")]
        public virtual ApplicationUserProfile ApplicationUserProfile { get; set; }

        public Guid MessageCategoryRefId { get; set; }
        
        [ForeignKey("MessageCategoryRefId")]
        public virtual MessageCategory Category { get; set; }
    }

    public class MessageDayOfWeekSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageDayOfWeekSettingId { get; set; }

        public bool Enabled { get; set; }

        [Required]
        public string Title { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int NumOfMessages { get; set; }

        public string ApplicationUserProfileRefId { get; set; }

        [ForeignKey("ApplicationUserProfileRefId")]
        public virtual ApplicationUserProfile ApplicationUserProfile { get; set; }
    }

    public class InviteBundle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid InviteBundleId { get; set; }
        [Required]
        public string AppStoreSku { get; set; }
        [Required]
        public string PlayStoreSku { get; set; }
        
        public int NumberOfInvites { get; set; }

        public decimal Cost { get; set; }
    }

    public class PostalCodeGeoLocation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid PostalCodeGeoLocationId { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string PlaceName { get; set; }

        public string AdminName1 { get; set; }

        public string AdminCode1 { get; set; }

        public string AdminName2 { get; set; }

        public string AdminCode2 { get; set; }

        public string AdminName3 { get; set; }

        public string AdminCode3 { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }


    }

    public class BibleVerse
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid BibleVerseId { get; set; }

        [Required]
        public string ShortCode { get; set; }

        [Required]
        public string Book { get; set; }

        [Required]
        public int Chapter { get; set; }

        [Required]
        public int Verse { get; set; }

        [Required]
        public string Text { get; set; }
    }
}