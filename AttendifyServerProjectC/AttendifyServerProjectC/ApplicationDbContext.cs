﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



namespace AttendifyServerProjectC
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TestTable> TestTable { get; set; }

        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
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

}
