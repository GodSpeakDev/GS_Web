namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveInviteBalance : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ApplicationUserProfiles", "InviteBalance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ApplicationUserProfiles", "InviteBalance", c => c.Int(nullable: false));
        }
    }
}
