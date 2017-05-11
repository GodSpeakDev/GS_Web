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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ApplicationUserProfileId { get; set; }
    
        public string Code { get; set; }

        [Required]
        public string UserId { get; set; }

        public string ReferringEmailAddress { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string Token { get; set; }
        
        public string PhotoUrl { get; set; }

        public virtual ICollection<MessageCategorySetting> MessageCategorySettings { get; set; }

       
        public virtual ICollection<MessageDayOfWeekSetting> MessageDayOfWeekSettings { get; set; }
    }

    public class ScheduledMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ScheduledMessageId { get; set; }

        public string UserId { get; set; }

        public string UserInviteCode { get; set; }

        public DateTime ScheduledDateTime { get; set; }

        public string VerseCode { get; set; }
    }

    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageId { get; set; }

        public string VerseCode { get; set; }

        public virtual ICollection<MessageCategory> Categories { get; set; }

        public Message()
        {
            Categories = new HashSet<MessageCategory>();
        }
    }

    public class MessageCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageCategoryId { get; set; }

        [Required]
        public string Title { get; set; }

        public MessageCategory()
        {
            Messages = new HashSet<Message>();
        }
        public virtual ICollection<Message> Messages{ get; set; }

    }

    public class MessageCategorySetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid MessageCategorySettingId { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }
        
        public Guid ApplicationUserProfileRefId { get; set; }

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

        public Guid ApplicationUserProfileRefId { get; set; }

        [ForeignKey("ApplicationUserProfileRefId")]
        public virtual ApplicationUserProfile ApplicationUserProfile { get; set; }
    }

    public class ImpactDay
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ImpactDayId { get; set; }

        [Required]
        public string InviteCode { get; set; }

        [Required]
        public DateTime Day { get; set; }

        [Required]
        public string DayTitle { get; set; }

        public virtual ICollection<ImpactDayGeoPoint> Points { get; set; }

        public virtual ICollection<ImpactDeliveredMessage> DeliveredMessages{ get; set; }


    }

    public class ImpactDeliveredMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ImpactDeliveredMessageId { get; set; }

        public Guid ImpactDayRefId { get; set; }

        [ForeignKey("ImpactDayRefId")]
        public virtual ImpactDay ImpactDay { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string VerseCode { get; set; }
    }

    public class ImpactDayGeoPoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid ImpactDayGeoPointId { get; set; }
        
        public Guid ImpactDayRefId { get; set; }

        [ForeignKey("ImpactDayRefId")]
        public virtual ImpactDay ImpactDay { get; set; }

        public int Count { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
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