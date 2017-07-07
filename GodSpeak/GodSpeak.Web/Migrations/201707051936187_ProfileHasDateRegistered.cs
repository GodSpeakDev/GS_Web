namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProfileHasDateRegistered : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUserProfiles", "DateRegistered", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUserProfiles", "DateRegistered");
        }
    }
}
