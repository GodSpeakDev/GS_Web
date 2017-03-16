namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationUserProfileHasMessageCategorySettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MessageCategorySettings",
                c => new
                    {
                        MessageCategorySettingId = c.Guid(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        Title = c.String(nullable: false),
                        ApplicationUserProfileRefId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MessageCategorySettingId)
                .ForeignKey("dbo.ApplicationUserProfiles", t => t.ApplicationUserProfileRefId)
                .Index(t => t.ApplicationUserProfileRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MessageCategorySettings", "ApplicationUserProfileRefId", "dbo.ApplicationUserProfiles");
            DropIndex("dbo.MessageCategorySettings", new[] { "ApplicationUserProfileRefId" });
            DropTable("dbo.MessageCategorySettings");
        }
    }
}
