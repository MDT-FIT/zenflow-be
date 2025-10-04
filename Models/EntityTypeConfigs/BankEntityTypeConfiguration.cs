using FintechStatsPlatform.Enumirators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FintechStatsPlatform.Models.EntityTypeConfigs
{
    public class BankEntityTypeConfiguration : IEntityTypeConfiguration<BankConfig>
    {
        public void Configure(EntityTypeBuilder<BankConfig> builder)
        {


            builder
                .ToTable("banks")
                .HasKey(b => b.Id);

            builder
                .Property(b => b.Id)
                .HasColumnName("id")
                .HasColumnType("varchar(100)");

            builder
                .Property(b => b.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .HasConversion(BankNameMapper.Map)
                .IsRequired();

            builder
                .Property(b => b.Logo)
                .HasColumnName("logo")
                .HasColumnType("text")
                .IsRequired();

            builder
                .Property(b => b.Currency)
                .HasColumnName("currency")
                .IsRequired();

            builder
                .Property(b => b.ApiLink)
                .HasColumnName("api_link")
                .HasMaxLength(300)
                .IsRequired();
            builder
              .Ignore(b => b.IsEnabled);
            builder
                .Ignore(b => b.Mapper);

            builder
                .Ignore(b => b.UpdatedAt);

            builder
                .Ignore(b => b.CreatedAt);
        }
    }
}