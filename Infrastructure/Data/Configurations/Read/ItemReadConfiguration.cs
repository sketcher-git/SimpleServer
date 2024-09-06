using Domain.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Read;

internal sealed class ItemReadConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.DataId);
        builder.Property(i => i.ItemCount);
        builder.Property(i => i.OwnerId).IsRequired();

        builder.HasIndex(i => i.OwnerId);
    }
}