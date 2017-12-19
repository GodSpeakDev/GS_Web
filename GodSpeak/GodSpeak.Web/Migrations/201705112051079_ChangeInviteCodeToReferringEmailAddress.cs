namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeInviteCodeToReferringEmailAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUserProfiles", "ReferringEmailAddress", c => c.String());
            DropColumn("dbo.ApplicationUserProfiles", "ReferringCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ApplicationUserProfiles", "ReferringCode", c => c.String());
            DropColumn("dbo.ApplicationUserProfiles", "ReferringEmailAddress");
        }
    }
}
