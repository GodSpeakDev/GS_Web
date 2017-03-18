namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AppHasBibleVerses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BibleVerses",
                c => new
                    {
                        BibleVerseId = c.Guid(nullable: false, identity: true),
                        ShortCode = c.String(nullable: false),
                        Book = c.String(nullable: false),
                        Chapter = c.Int(nullable: false),
                        Verse = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.BibleVerseId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BibleVerses");
        }
    }
}
