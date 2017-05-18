namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeImpactDayInviteCodeToEmailAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImpactDays", "EmailAddress", c => c.String(nullable: false));
            DropColumn("dbo.ImpactDays", "InviteCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ImpactDays", "InviteCode", c => c.String(nullable: false));
            DropColumn("dbo.ImpactDays", "EmailAddress");
        }
    }
}
