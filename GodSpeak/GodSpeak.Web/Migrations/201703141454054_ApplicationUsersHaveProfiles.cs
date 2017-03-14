namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationUsersHaveProfiles : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ApplicationUserInvites", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserInvites", new[] { "ApplicationUser_Id" });
            CreateTable(
                "dbo.ApplicationUserProfiles",
                c => new
                    {
                        ApplicationUserProfileId = c.Guid(nullable: false, identity: true),
                        Code = c.String(nullable: false),
                        InviteBalance = c.Int(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        CountryCode = c.String(nullable: false),
                        PostalCode = c.String(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ApplicationUserProfileId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            DropTable("dbo.ApplicationUserInvites");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ApplicationUserInvites",
                c => new
                    {
                        ApplicationUserInviteId = c.Guid(nullable: false, identity: true),
                        Code = c.String(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ApplicationUserInviteId);
            
            DropForeignKey("dbo.ApplicationUserProfiles", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserProfiles", new[] { "ApplicationUser_Id" });
            DropTable("dbo.ApplicationUserProfiles");
            CreateIndex("dbo.ApplicationUserInvites", "ApplicationUser_Id");
            AddForeignKey("dbo.ApplicationUserInvites", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
