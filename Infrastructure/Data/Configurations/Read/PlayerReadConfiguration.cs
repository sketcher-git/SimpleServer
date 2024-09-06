using Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Read;

internal sealed class PlayerReadConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        builder.ComplexProperty(
            p => p.Email,
            email => email.Property(e => e.Value)
            .HasColumnName(nameof(Player.Email)));

        builder.ComplexProperty(
            p => p.Name,
            name => name.Property(n => n.Value)
                            .HasColumnName(nameof(Player.Name))
                            .HasMaxLength(255));
    }
}