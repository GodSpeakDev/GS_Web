namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationUserProfileHasMessageDayOfWeekSetting : DbMigration
    {
        public override void Up()
        {
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
                        ApplicationUserProfileRefId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MessageDayOfWeekSettingId)
                .ForeignKey("dbo.ApplicationUserProfiles", t => t.ApplicationUserProfileRefId)
                .Index(t => t.ApplicationUserProfileRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MessageDayOfWeekSettings", "ApplicationUserProfileRefId", "dbo.ApplicationUserProfiles");
            DropIndex("dbo.MessageDayOfWeekSettings", new[] { "ApplicationUserProfileRefId" });
            DropTable("dbo.MessageDayOfWeekSettings");
        }
    }
}
