namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppV1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImpactDays",
                c => new
                    {
                        ImpactDayId = c.Guid(nullable: false, identity: true),
                        InviteCode = c.String(nullable: false),
                        Day = c.DateTime(nullable: false),
                        DayTitle = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ImpactDayId);
            
            CreateTable(
                "dbo.ImpactDeliveredMessages",
                c => new
                    {
                        ImpactDeliveredMessageId = c.Guid(nullable: false, identity: true),
                        ImpactDayRefId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false),
                        VerseCode = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ImpactDeliveredMessageId)
                .ForeignKey("dbo.ImpactDays", t => t.ImpactDayRefId, cascadeDelete: true)
                .Index(t => t.ImpactDayRefId);
            
            CreateTable(
                "dbo.ImpactDayGeoPoints",
                c => new
                    {
                        ImpactDayGeoPointId = c.Guid(nullable: false, identity: true),
                        ImpactDayRefId = c.Guid(nullable: false),
                        Count = c.Int(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ImpactDayGeoPointId)
                .ForeignKey("dbo.ImpactDays", t => t.ImpactDayRefId, cascadeDelete: true)
                .Index(t => t.ImpactDayRefId);
            
            CreateTable(
                "dbo.InviteBundles",
                c => new
                    {
                        InviteBundleId = c.Guid(nullable: false, identity: true),
                        AppStoreSku = c.String(nullable: false),
                        PlayStoreSku = c.String(nullable: false),
                        NumberOfInvites = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InviteBundleId);
            
            CreateTable(
                "dbo.MessageCategories",
                c => new
                    {
                        MessageCategoryId = c.Guid(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MessageCategoryId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Guid(nullable: false, identity: true),
                        VerseCode = c.String(),
                    })
                .PrimaryKey(t => t.MessageId);
            
            CreateTable(
                "dbo.ApplicationUserProfiles",
                c => new
                    {
                        ApplicationUserProfileId = c.Guid(nullable: false, identity: true),
                        Code = c.String(nullable: false),
                        UserId = c.String(nullable: false),
                        ReferringCode = c.String(),
                        InviteBalance = c.Int(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        CountryCode = c.String(nullable: false),
                        PostalCode = c.String(nullable: false),
                        Token = c.String(),
                        PhotoUrl = c.String(),
                    })
                .PrimaryKey(t => t.ApplicationUserProfileId);
            
            CreateTable(
                "dbo.MessageCategorySettings",
                c => new
                    {
                        MessageCategorySettingId = c.Guid(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        ApplicationUserProfileRefId = c.Guid(nullable: false),
                        MessageCategoryRefId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.MessageCategorySettingId)
                .ForeignKey("dbo.ApplicationUserProfiles", t => t.ApplicationUserProfileRefId, cascadeDelete: true)
                .ForeignKey("dbo.MessageCategories", t => t.MessageCategoryRefId, cascadeDelete: true)
                .Index(t => t.ApplicationUserProfileRefId)
                .Index(t => t.MessageCategoryRefId);
            
            CreateTable(
                "dbo.MessageDayOfWeekSettings",
                c => new
                    {
                        MessageDayOfWeekSettingId = c.Guid(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        Title = c.String(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        NumOfMessages = c.Int(nullable: false),
                        ApplicationUserProfileRefId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.MessageDayOfWeekSettingId)
                .ForeignKey("dbo.ApplicationUserProfiles", t => t.ApplicationUserProfileRefId, cascadeDelete: true)
                .Index(t => t.ApplicationUserProfileRefId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ScheduledMessages",
                c => new
                    {
                        ScheduledMessageId = c.Guid(nullable: false, identity: true),
                        UserId = c.String(),
                        UserInviteCode = c.String(),
                        ScheduledDateTime = c.DateTime(nullable: false),
                        VerseCode = c.String(),
                    })
                .PrimaryKey(t => t.ScheduledMessageId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MessageMessageCategories",
                c => new
                    {
                        Message_MessageId = c.Guid(nullable: false),
                        MessageCategory_MessageCategoryId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Message_MessageId, t.MessageCategory_MessageCategoryId })
                .ForeignKey("dbo.Messages", t => t.Message_MessageId, cascadeDelete: true)
                .ForeignKey("dbo.MessageCategories", t => t.MessageCategory_MessageCategoryId, cascadeDelete: true)
                .Index(t => t.Message_MessageId)
                .Index(t => t.MessageCategory_MessageCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.MessageDayOfWeekSettings", "ApplicationUserProfileRefId", "dbo.ApplicationUserProfiles");
            DropForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories");
            DropForeignKey("dbo.MessageCategorySettings", "ApplicationUserProfileRefId", "dbo.ApplicationUserProfiles");
            DropForeignKey("dbo.MessageMessageCategories", "MessageCategory_MessageCategoryId", "dbo.MessageCategories");
            DropForeignKey("dbo.MessageMessageCategories", "Message_MessageId", "dbo.Messages");
            DropForeignKey("dbo.ImpactDayGeoPoints", "ImpactDayRefId", "dbo.ImpactDays");
            DropForeignKey("dbo.ImpactDeliveredMessages", "ImpactDayRefId", "dbo.ImpactDays");
            DropIndex("dbo.MessageMessageCategories", new[] { "MessageCategory_MessageCategoryId" });
            DropIndex("dbo.MessageMessageCategories", new[] { "Message_MessageId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.MessageDayOfWeekSettings", new[] { "ApplicationUserProfileRefId" });
            DropIndex("dbo.MessageCategorySettings", new[] { "MessageCategoryRefId" });
            DropIndex("dbo.MessageCategorySettings", new[] { "ApplicationUserProfileRefId" });
            DropIndex("dbo.ImpactDayGeoPoints", new[] { "ImpactDayRefId" });
            DropIndex("dbo.ImpactDeliveredMessages", new[] { "ImpactDayRefId" });
            DropTable("dbo.MessageMessageCategories");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ScheduledMessages");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.MessageDayOfWeekSettings");
            DropTable("dbo.MessageCategorySettings");
            DropTable("dbo.ApplicationUserProfiles");
            DropTable("dbo.Messages");
            DropTable("dbo.MessageCategories");
            DropTable("dbo.InviteBundles");
            DropTable("dbo.ImpactDayGeoPoints");
            DropTable("dbo.ImpactDeliveredMessages");
            DropTable("dbo.ImpactDays");
        }
    }
}
