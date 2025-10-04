using FintechStatsPlatform.Models.EntityTypeConfigs;
using Microsoft.EntityFrameworkCore;

namespace FintechStatsPlatform.Models
{
    public class FintechContext(DbContextOptions<FintechContext> options)
        : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<BankConfig> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BankAccountEntityTypeConfiguration bankAccountConfig = new();
            BankEntityTypeConfiguration bankConfig = new();
            UserEntityTypeConfiguration userConfig = new();

            bankAccountConfig.Configure(modelBuilder.Entity<BankAccount>());
            bankConfig.Configure(modelBuilder.Entity<BankConfig>());
            userConfig.Configure(modelBuilder.Entity<User>());
        }
    }
}