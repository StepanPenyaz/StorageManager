using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Models;

namespace Storage.Data.Configurations;

public class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("Lots");

        builder.HasKey(lot => lot.Id);

        builder.Property(lot => lot.Id).ValueGeneratedOnAdd();

        builder.Property(lot => lot.ItemId).IsRequired().HasMaxLength(100);

        builder.HasMany(lot => lot.LotSections)
            .WithOne()
            .HasForeignKey(lotSection => lotSection.LotId);

        builder.Navigation(lot => lot.LotSections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
