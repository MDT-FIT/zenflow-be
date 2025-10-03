using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FintechStatsPlatform.Models.EntityTypeConfigs
{
    public class BankAccountEntityTypeConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder
                .ToTable("bank_accounts")
                .HasKey(a => a.Id);

            builder
                .Property(a => a.Id)
                .HasColumnName("id")
                .HasColumnType("varchar(100)");

            builder
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(a => a.Bank)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(a => a.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(a => a.UserId)
                .HasColumnName("user_id");

            builder
                .Property(a => a.BankId)
                .HasColumnName("bank_id");

            builder
                .Property(a => a.Balance)
                .HasColumnName("balance")
                .HasColumnType("numeric(10, 2)")
                .HasDefaultValue(0);

            builder
                .Property(a => a.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            builder
                .Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder
                .Ignore(b => b.Mapper);
        }
    }
}
