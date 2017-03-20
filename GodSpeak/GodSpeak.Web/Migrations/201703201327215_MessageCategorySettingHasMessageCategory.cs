namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageCategorySettingHasMessageCategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MessageCategories",
                c => new
                    {
                        MessageCategoryId = c.Guid(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MessageCategoryId);
            
            AddColumn("dbo.MessageCategorySettings", "MessageCategoryRefId", c => c.Guid(nullable: false));
            CreateIndex("dbo.MessageCategorySettings", "MessageCategoryRefId");
            AddForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories", "MessageCategoryId", cascadeDelete: true);
            DropColumn("dbo.MessageCategorySettings", "Title");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MessageCategorySettings", "Title", c => c.String(nullable: false));
            DropForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories");
            DropIndex("dbo.MessageCategorySettings", new[] { "MessageCategoryRefId" });
            DropColumn("dbo.MessageCategorySettings", "MessageCategoryRefId");
            DropTable("dbo.MessageCategories");
        }
    }
}
