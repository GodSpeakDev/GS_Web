namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PostalCodeNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ApplicationUserProfiles", "PostalCode", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ApplicationUserProfiles", "PostalCode", c => c.String(nullable: false));
        }
    }
}
