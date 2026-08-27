using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Infrastructure.Data.Mappings;

public class GameMapping : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");
        builder.HasKey(g => g.Id);
        
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.CreatedAt).HasColumnName("created_at");
        builder.Property(g => g.UpdatedAt).HasColumnName("updated_at");

        builder.ComplexProperty(g => g.Title, titleBuilder =>
        {
            titleBuilder.Property(t => t.Value)
                .HasColumnName("title")
                .HasMaxLength(GameTitle.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(g => g.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount) 
                .HasColumnName("price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();
        });

        builder.Property(g => g.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(g => g.Genre)
            .HasColumnName("genre")
            .HasConversion<string>() 
            .IsRequired();

        builder.Property(g => g.Status)
            .HasColumnName("status")
            .HasConversion<string>() 
            .IsRequired();

        builder.Property(g => g.ReleaseDate)
            .HasColumnName("release_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Ignore(g => g.DomainEvents);
    }
}