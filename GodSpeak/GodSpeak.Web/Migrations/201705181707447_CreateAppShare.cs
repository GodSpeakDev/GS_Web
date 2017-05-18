namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateAppShare : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppShares",
                c => new
                    {
                        AppShareId = c.Guid(nullable: false, identity: true),
                        From = c.String(nullable: false),
                        To = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AppShareId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AppShares");
        }
    }
}
