namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReAddInviteBalanceToProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUserProfiles", "InviteBalance", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUserProfiles", "InviteBalance");
        }
    }
}
