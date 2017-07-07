namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastNameIsOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ApplicationUserProfiles", "LastName", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ApplicationUserProfiles", "LastName", c => c.String(nullable: false));
        }
    }
}
