using Cloud.Data.Tables;
using Cloud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!Directory.Exists("Data/Base"))
            {
                Directory.CreateDirectory("Data/Base");
            }
            optionsBuilder.UseSqlite("Data Source = Data/Base/Database.db");
        }
        public DbSet<UserUploades> Uploads {get;set;}
    }
}