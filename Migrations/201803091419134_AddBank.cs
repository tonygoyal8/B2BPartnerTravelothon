namespace B2BPartnerTravelothon.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBank : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Payments", newName: "PPayments");
            CreateTable(
                "dbo.PUserRegistrationPlans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        UserRoleId = c.Int(nullable: false),
                        MemberType = c.String(),
                        PlanType = c.String(),
                        Fee = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Remarks = c.String(),
                        Recharge = c.Boolean(nullable: false),
                        Flight = c.Boolean(nullable: false),
                        DMR = c.Boolean(nullable: false),
                        Rail = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PUserProfile", "Agency", c => c.String());
            AddColumn("dbo.PUserProfile", "Balance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PUserProfile", "PlanId", c => c.Int(nullable: false));
            AddColumn("dbo.PUserProfile", "ParentId", c => c.Int(nullable: false));
            AddColumn("dbo.PUserProfile", "PaymentId", c => c.Int(nullable: false));
            AddColumn("dbo.PUserProfile", "PhoneNumber", c => c.String());
            AddColumn("dbo.PUserProfile", "Email", c => c.String());
            AlterColumn("dbo.PUserProfile", "BirthDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.PUserProfile", "ReferralId");
            DropColumn("dbo.PUserProfile", "ReferredById");
            DropColumn("dbo.PUserProfile", "Points");
            DropColumn("dbo.PUserProfile", "FirstTransaction");
            DropColumn("dbo.PUserProfile", "Subscribe");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PUserProfile", "Subscribe", c => c.Boolean(nullable: false));
            AddColumn("dbo.PUserProfile", "FirstTransaction", c => c.Boolean(nullable: false));
            AddColumn("dbo.PUserProfile", "Points", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PUserProfile", "ReferredById", c => c.String());
            AddColumn("dbo.PUserProfile", "ReferralId", c => c.String());
            AlterColumn("dbo.PUserProfile", "BirthDate", c => c.DateTime());
            DropColumn("dbo.PUserProfile", "Email");
            DropColumn("dbo.PUserProfile", "PhoneNumber");
            DropColumn("dbo.PUserProfile", "PaymentId");
            DropColumn("dbo.PUserProfile", "ParentId");
            DropColumn("dbo.PUserProfile", "PlanId");
            DropColumn("dbo.PUserProfile", "Balance");
            DropColumn("dbo.PUserProfile", "Agency");
            DropTable("dbo.PUserRegistrationPlans");
            RenameTable(name: "dbo.PPayments", newName: "Payments");
        }
    }
}
