using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using B2BPartnerTravelothon.Models.Flight;
using B2BPartnerTravelothon.Models.User;
using B2BPartnerTravelothon.Models.Payment;
using B2BPartnerTravelothon.Models.Bank;
using B2BPartnerTravelothon.Models.Operator;
using B2BPartnerTravelothon.Models.Markup;
using B2BPartnerTravelothon.Models.Helper;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using B2BPartnerTravelothon.Models.Passbook;
using B2BPartnerTravelothon.Models.Cases;

namespace B2BPartnerTravelothon.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
    //public class ApplicationUser : IdentityUser {  }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
         public DbSet<AirportDto> Airports { get; set; }
        public DbSet<PUserProfileDto> PUserProfile { get; set; }
        public DbSet<PaymentDto> Payments { get; set; }
        public DbSet<PFlightDto> PFlights { get; set; }
        public DbSet<PFlightDetailsDto> PFlightDetails { get; set; }
       public DbSet<PFlightSegmentsDto> PFlightSegments { get; set; }
        public DbSet<PUserRegistrationPlansDto> PUserRegistrationPlans { get; set; }
        public DbSet<PBanksDto> Banks { get; set; }
        public DbSet<PBalanceRequestDto> BalanceRequests { get; set; }
        public DbSet<OperatorsDto> Operators { get; set; }
        public DbSet<MarkupDto> Markups { get; set; }
        public DbSet<PassBookDto> PassBook { get; set; }
        public DbSet<UserCasesDto> UserCases { get; set; }
        public virtual ObjectResult<OperatorMappingHelper> SP_B2B_Agent_Details(string UId)
        {
            return ((IObjectContextAdapter)(this)).ObjectContext.ExecuteStoreQuery<OperatorMappingHelper>("sp_b2b_agent_details @UId", new SqlParameter("UId",UId));
        }

        public virtual void SP_B2B_Passbook(int FId,int ServiceType)
        {
            ((IObjectContextAdapter)(this)).ObjectContext.ExecuteStoreCommand("SP_B2B_Passbook @FlightId, @ServiceType",new SqlParameter("FlightId", FId), new SqlParameter("ServiceType", ServiceType));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This needs to go before the other rules!

            modelBuilder.Entity<ApplicationUser>().ToTable("PUsers");
            modelBuilder.Entity<IdentityRole>().ToTable("PRoles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("PUserRoles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("PUserClaims");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("PUserLogins");
        }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}