namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteInviteBundleModel : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.InviteBundles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.InviteBundles",
                c => new
                    {
                        InviteBundleId = c.Guid(nullable: false, identity: true),
                        AppStoreSku = c.String(nullable: false),
                        PlayStoreSku = c.String(nullable: false),
                        NumberOfInvites = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InviteBundleId);
            
        }
    }
}
