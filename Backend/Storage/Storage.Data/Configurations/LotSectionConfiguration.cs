using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Models;

namespace Storage.Data.Configurations;

public class LotSectionConfiguration : IEntityTypeConfiguration<LotSection>
{
    public void Configure(EntityTypeBuilder<LotSection> builder)
    {
        builder.ToTable("LotSections");

        builder.HasKey(lotSection => new { lotSection.LotId, lotSection.SectionId });

        builder.Property(lotSection => lotSection.Quantity).IsRequired();

        builder.HasOne<Lot>()
            .WithMany(lot => lot.LotSections)
            .HasForeignKey(lotSection => lotSection.LotId);

        builder.HasOne<Section>()
            .WithMany(section => section.LotSections)
            .HasForeignKey(lotSection => lotSection.SectionId);
    }
}
