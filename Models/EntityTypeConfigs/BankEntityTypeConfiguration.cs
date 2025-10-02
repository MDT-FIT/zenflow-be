using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FintechStatsPlatform.Models.EntityTypeConfigs
{
    public class BankEntityTypeConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder
                .ToTable("banks")
                .HasKey(b =>  b.Id);

            builder
                .Property(b => b.Id)
                .HasColumnName("id")
                .HasColumnType("varchar(100)");

            builder
                .Property(b => b.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder
                .Property(b => b.Logo)
                .HasColumnName("logo")
                .HasColumnType("text")
                .IsRequired();

            builder
                .Property(b => b.Country)
                .HasColumnName("country")
                .HasConversion<string>();

            builder
                .Property(b => b.ApiLink)
                .HasColumnName("api_link")
                .HasMaxLength(300)
                .IsRequired();

            builder
                .Ignore(b => b.UpdatedAt);

            builder
                .Ignore(b => b.CreatedAt);
        }
    }
}
