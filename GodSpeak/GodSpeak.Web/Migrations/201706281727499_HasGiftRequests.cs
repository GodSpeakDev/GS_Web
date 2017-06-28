namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HasGiftRequests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GiftRequests",
                c => new
                    {
                        GiftRequestId = c.Guid(nullable: false, identity: true),
                        Email = c.String(nullable: false),
                        Platform = c.Int(nullable: false),
                        ReferringCode = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.GiftRequestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GiftRequests");
        }
    }
}
