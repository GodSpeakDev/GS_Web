namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PayPalTransactionHasInviteCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PayPalTransactions", "InviteCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PayPalTransactions", "InviteCount");
        }
    }
}
