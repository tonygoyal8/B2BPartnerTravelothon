namespace B2BPartnerTravelothon.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Airlines",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Logo = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "dbo.Airports",
                c => new
                    {
                        IATACode = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.IATACode);
            
            CreateTable(
                "dbo.Offers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        PromoCode = c.String(),
                        Value = c.Int(nullable: false),
                        PartnerId = c.Int(nullable: false),
                        ServiceId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Description = c.String(),
                        MaxUsage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentRequestId = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        Purpose = c.String(),
                        Amount = c.Decimal(precision: 18, scale: 2),
                        Fees = c.Decimal(precision: 18, scale: 2),
                        MAC = c.String(),
                        Status = c.Int(),
                        PaymentId = c.String(),
                    })
                .PrimaryKey(t => t.PaymentRequestId);
            
            CreateTable(
                "dbo.TFlight",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        APIId = c.Int(nullable: false),
                        UserId = c.String(),
                        OfferId = c.Int(nullable: false),
                        Type = c.String(),
                        Trip = c.String(),
                        Origin = c.String(),
                        Destination = c.String(),
                        DOJ = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FareType = c.String(),
                        Adult = c.Int(nullable: false),
                        Child = c.Int(nullable: false),
                        Infant = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        UserTrackId = c.String(),
                        HermesPnr = c.String(),
                        ReferenceNumber = c.String(),
                        ServiceType = c.Int(nullable: false),
                        PaymentId = c.Int(nullable: false),
                        Contact = c.String(),
                        Email = c.String(),
                        GSTNumber = c.String(),
                        Company = c.String(),
                        PointsEarned = c.Int(nullable: false),
                        PointsUsed = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PUserProfile",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        ReferralId = c.String(),
                        ReferredById = c.String(),
                        Points = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Country = c.String(),
                        State = c.String(),
                        City = c.String(),
                        BirthDate = c.DateTime(),
                        Gender = c.String(),
                        PanCard = c.String(),
                        GST = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Aadhar = c.String(),
                        FirstTransaction = c.Boolean(nullable: false),
                        Subscribe = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.PRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.PUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.PRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.PUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.TAirlineDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        TFlightId = c.Int(nullable: false),
                        Airline = c.String(),
                        AirlineCode = c.String(),
                        PNR = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TFlightDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        TFlightId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Type = c.String(),
                        Age = c.Int(nullable: false),
                        FrequentFlyerNumber = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        TicketNumber = c.String(),
                        Title = c.String(),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TFlightSegments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                        TFlightDetailsId = c.Int(nullable: false),
                        TicketNumber = c.String(),
                        FlightNumber = c.String(),
                        AirCraftType = c.String(),
                        Origin = c.String(),
                        Destination = c.String(),
                        OriginAirportTerminal = c.String(),
                        DestinationAirportTerminal = c.String(),
                        DepartureDateTime = c.DateTime(nullable: false),
                        ArrivalDatetime = c.DateTime(nullable: false),
                        AirlineCode = c.String(),
                        ClassCode = c.String(),
                        ClassCodeDesc = c.String(),
                        BasicAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EquivalentFare = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalTaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ServiceCharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.PUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.PUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PUserRoles", "UserId", "dbo.PUsers");
            DropForeignKey("dbo.PUserLogins", "UserId", "dbo.PUsers");
            DropForeignKey("dbo.PUserClaims", "UserId", "dbo.PUsers");
            DropForeignKey("dbo.PUserRoles", "RoleId", "dbo.PRoles");
            DropIndex("dbo.PUserLogins", new[] { "UserId" });
            DropIndex("dbo.PUserClaims", new[] { "UserId" });
            DropIndex("dbo.PUsers", "UserNameIndex");
            DropIndex("dbo.PUserRoles", new[] { "RoleId" });
            DropIndex("dbo.PUserRoles", new[] { "UserId" });
            DropIndex("dbo.PRoles", "RoleNameIndex");
            DropTable("dbo.PUserLogins");
            DropTable("dbo.PUserClaims");
            DropTable("dbo.PUsers");
            DropTable("dbo.TFlightSegments");
            DropTable("dbo.TFlightDetails");
            DropTable("dbo.TAirlineDetails");
            DropTable("dbo.PUserRoles");
            DropTable("dbo.PRoles");
            DropTable("dbo.PUserProfile");
            DropTable("dbo.TFlight");
            DropTable("dbo.Payments");
            DropTable("dbo.Offers");
            DropTable("dbo.Airports");
            DropTable("dbo.Airlines");
        }
    }
}
