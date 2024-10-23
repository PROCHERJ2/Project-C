using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AttendifySharedProjectC.Models;
using Microsoft.Extensions.Logging;


namespace AttendifyServerProjectC
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TestTable> TestTable { get; set; }

        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        public DbSet<UserRoleVerification> UserRoleVerifications { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventDay> EventDays { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

    }

    public class TestTable   //this is temp, remove this later after db connection test
    {
        public int Id { get; set; }
        public string Test { get; set; }
    }

    public class EmailVerificationToken
    {
        [Key]
        public string VerificationToken { get; set; }  

        public DateTime TokenCreationDate { get; set; } 
    }

    public class UserRoleVerification
    {
        [Key]
        public string UserId { get; set; }

        public string RequestedRole { get; set; }

        public string VerificationStatus { get; set; } = "Pending";

        public DateTime DateRequested { get; set; } = DateTime.UtcNow;
    }
}
