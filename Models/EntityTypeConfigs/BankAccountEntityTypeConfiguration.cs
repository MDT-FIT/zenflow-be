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
                .Property(a => a.UserId)
                .HasColumnName("user_id");

            builder
                .Property(a => a.BankId)
                .HasColumnName("bank_id");

            builder
                .Property(a => a.CurrencyScale)
                .HasColumnName("currency_scale")
                .HasColumnType("smallint")
                .HasDefaultValue(2);

            builder
                .Property(a => a.Balance)
                .HasColumnName("balance")
                .HasColumnType("bigint")
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
            builder
                .Ignore(b => b.Bank);
        }
    }
}
