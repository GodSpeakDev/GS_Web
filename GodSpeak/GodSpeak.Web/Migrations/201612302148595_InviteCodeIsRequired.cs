namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InviteCodeIsRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ApplicationUserInvites", "Code", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ApplicationUserInvites", "Code", c => c.String());
        }
    }
}
