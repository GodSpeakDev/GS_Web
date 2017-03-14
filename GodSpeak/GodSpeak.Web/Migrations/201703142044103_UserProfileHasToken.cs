namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserProfileHasToken : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUserProfiles", "Token", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUserProfiles", "Token");
        }
    }
}
