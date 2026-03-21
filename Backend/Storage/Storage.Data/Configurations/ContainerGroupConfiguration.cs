using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Models;

namespace Storage.Data.Configurations;

public class ContainerGroupConfiguration : IEntityTypeConfiguration<ContainerGroup>
{
    public void Configure(EntityTypeBuilder<ContainerGroup> builder)
    {
        builder.ToTable("ContainerGroups");

        builder.HasKey(group => group.Id);

        builder.Property(group => group.Id).ValueGeneratedOnAdd();

        builder.Property(group => group.Type).IsRequired();

        builder.Property(group => group.Cabinet).IsRequired();

        builder.Property(group => group.Shelf).IsRequired();

        builder.Property(group => group.GroupRow).IsRequired();

        builder.Property(group => group.GroupColumn).IsRequired();

        builder.HasMany(group => group.Containers)
            .WithOne()
            .HasForeignKey(container => container.ContainerGroupId);

        builder.Navigation(group => group.Containers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
