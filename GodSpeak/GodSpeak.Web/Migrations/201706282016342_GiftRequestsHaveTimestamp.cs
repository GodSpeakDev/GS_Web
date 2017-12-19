namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GiftRequestsHaveTimestamp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftRequests", "DateTimeRequested", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GiftRequests", "DateTimeRequested");
        }
    }
}
