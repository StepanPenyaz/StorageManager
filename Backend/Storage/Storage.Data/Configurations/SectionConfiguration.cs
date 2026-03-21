using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Models;

namespace Storage.Data.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Id).ValueGeneratedOnAdd();

        builder.Property(section => section.Index).IsRequired();

        builder.Property(section => section.ContainerId).IsRequired();

        builder.HasMany(section => section.LotSections)
            .WithOne()
            .HasForeignKey(lotSection => lotSection.SectionId);

        builder.Navigation(section => section.LotSections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
