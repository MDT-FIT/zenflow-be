using FintechStatsPlatform.Models.EntityTypeConfigs;
using Microsoft.EntityFrameworkCore;

namespace FintechStatsPlatform.Models
{
    public class FintechContext(DbContextOptions<FintechContext> options) 
        : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BankAccountEntityTypeConfiguration bankAccountConfig = 
                new BankAccountEntityTypeConfiguration();
            BankEntityTypeConfiguration bankConfig = new BankEntityTypeConfiguration();
            UserEntityTypeConfiguration userConfig = new UserEntityTypeConfiguration();

            bankAccountConfig.Configure(modelBuilder.Entity<BankAccount>());
            bankConfig.Configure(modelBuilder.Entity<Bank>());
            userConfig.Configure(modelBuilder.Entity<User>());
        }
    }
}
