using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Models;

namespace Storage.Data.Configurations;

public class ContainerConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.ToTable("Containers");

        builder.HasKey(container => container.Id);

        builder.Property(container => container.Id).ValueGeneratedOnAdd();

        builder.Property(container => container.Number).IsRequired();

        builder.Property(container => container.Type).IsRequired();

        builder.Property(container => container.ContainerGroupId).IsRequired();

        builder.HasOne<ContainerGroup>()
            .WithMany(group => group.Containers)
            .HasForeignKey(container => container.ContainerGroupId);

        builder.HasMany(container => container.Sections)
            .WithOne()
            .HasForeignKey(section => section.ContainerId);

        builder.Navigation(container => container.Sections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(container => container.Location, location =>
        {
            location.Property(value => value.Cabinet).HasColumnName("Cabinet").IsRequired();
            location.Property(value => value.Shelf).HasColumnName("Shelf").IsRequired();
            location.Property(value => value.GroupRow).HasColumnName("GroupRow").IsRequired();
            location.Property(value => value.GroupColumn).HasColumnName("GroupColumn").IsRequired();
            location.Property(value => value.PositionRow).HasColumnName("PositionRow").IsRequired();
            location.Property(value => value.PositionColumn).HasColumnName("PositionColumn").IsRequired();
        });
    }
}
