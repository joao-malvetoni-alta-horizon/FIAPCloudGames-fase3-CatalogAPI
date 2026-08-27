using CatalogAPI.Domain.Contexts.Libraries.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Infrastructure.Data.Mappings;

public class LibraryMapping : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.ToTable("library");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.HasIndex(x => new { x.UserId, x.GameId })
            .HasDatabaseName("ix_library_user_id_game_id")
            .IsUnique();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.Property(x => x.AcquiredOn)
            .HasColumnName("acquired_on")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}