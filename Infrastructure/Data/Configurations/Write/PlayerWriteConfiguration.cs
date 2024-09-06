using Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Write;

internal sealed class PlayerWriteConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255);

            email.HasIndex(e => e.Value)
                .HasDatabaseName("IX_Players_Email");
        });

        builder.OwnsOne(p => p.Name, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Name")
                .HasMaxLength(255);

            email.HasIndex(e => e.Value)
                .HasDatabaseName("IX_Players_Name");
        });
    }
}