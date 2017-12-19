namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HasPayPalTransactions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PayPalTransactions",
                c => new
                    {
                        PayPalTransactionId = c.Guid(nullable: false, identity: true),
                        PayPalPaymentId = c.String(nullable: false),
                        PayPalTransactionAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InviteCode = c.String(nullable: false),
                        DateTimePurchased = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PayPalTransactionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PayPalTransactions");
        }
    }
}
