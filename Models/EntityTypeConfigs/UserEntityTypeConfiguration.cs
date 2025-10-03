using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace FintechStatsPlatform.Models.EntityTypeConfigs
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .ToTable("users")
                .HasKey(u => u.Id);

            builder
                .Property(u => u.Id)
                .HasColumnName("id")
                .HasColumnType("varchar(100)");

            builder
                .Property(u => u.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            builder
                .Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(50)
                .IsRequired();

            builder
                .Property(u => u.AccountIds)
                .HasColumnName("account_id")
                .HasColumnType("varchar(100)[]")
                .HasDefaultValueSql("'{}'::varchar[]");

            builder
                .Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder
                .Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder
                .Property(u => u.PasswordHash)
                .HasColumnName("password")
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder
                .Ignore(b => b.Mapper);
        }
    }
}
