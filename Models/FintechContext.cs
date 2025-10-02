using Microsoft.EntityFrameworkCore;

namespace FintechStatsPlatform.Models
{
    public class FintechContext(DbContextOptions<FintechContext> options) 
        : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
    }
}
