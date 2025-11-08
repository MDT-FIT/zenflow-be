using FintechStatsPlatform.Models.EntityTypeConfigs;
using Microsoft.EntityFrameworkCore;

namespace FintechStatsPlatform.Models
{
    public class FintechContext(DbContextOptions<FintechContext> options) : DbContext(options)
    {
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<BankConfig> Banks { get; set; } = null!;
        public virtual DbSet<BankAccount> BankAccounts { get; set; } = null!;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Design",
            "CA1062:Validate arguments of public methods",
            Justification = "EF Core guarantees non-null parameter in OnModelCreating override."
        )]
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
