namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageCategorySettingHasMessageCategory1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories");
            DropIndex("dbo.MessageCategorySettings", new[] { "MessageCategoryRefId" });
            DropPrimaryKey("dbo.MessageCategories");
            AlterColumn("dbo.MessageCategorySettings", "MessageCategoryRefId", c => c.Guid(nullable: false));
            AlterColumn("dbo.MessageCategories", "MessageCategoryId", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("dbo.MessageCategories", "MessageCategoryId");
            CreateIndex("dbo.MessageCategorySettings", "MessageCategoryRefId");
            AddForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories", "MessageCategoryId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories");
            DropIndex("dbo.MessageCategorySettings", new[] { "MessageCategoryRefId" });
            DropPrimaryKey("dbo.MessageCategories");
            AlterColumn("dbo.MessageCategories", "MessageCategoryId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.MessageCategorySettings", "MessageCategoryRefId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.MessageCategories", "MessageCategoryId");
            CreateIndex("dbo.MessageCategorySettings", "MessageCategoryRefId");
            AddForeignKey("dbo.MessageCategorySettings", "MessageCategoryRefId", "dbo.MessageCategories", "MessageCategoryId");
        }
    }
}
