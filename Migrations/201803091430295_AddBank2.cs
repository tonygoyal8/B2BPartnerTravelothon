namespace B2BPartnerTravelothon.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBank2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PBalanceRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AmountApproved = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Mode = c.String(),
                        PBankId = c.Int(nullable: false),
                        TransactionId = c.String(),
                        Status = c.Int(nullable: false),
                        DepositorsName = c.String(),
                        DepositorsBank = c.String(),
                        DepositorsACNo = c.String(),
                        DepositDate = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        ParentUserId = c.Int(nullable: false),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PBanks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        Branch = c.String(),
                        AccountName = c.String(),
                        AccountNo = c.String(),
                        IFSCCode = c.String(),
                        UserId = c.Int(nullable: false),
                        Remarks = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PBanks");
            DropTable("dbo.PBalanceRequest");
        }
    }
}
