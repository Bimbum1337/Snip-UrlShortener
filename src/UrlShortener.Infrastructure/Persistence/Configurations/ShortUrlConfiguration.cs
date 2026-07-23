using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

public sealed class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.ToTable("ShortUrls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(ShortCode.MaxLength)
            .IsRequired();

        // This index, not the allocator, is what actually guarantees uniqueness
        // when two requests generate the same code at the same moment.
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_ShortUrls_Code");

        builder.Property(x => x.Destination)
            .HasMaxLength(DestinationUrl.MaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.VisitCount).IsRequired().HasDefaultValue(0L);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsCustomAlias).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasMany(x => x.Visits)
            .WithOne(v => v.ShortUrl!)
            .HasForeignKey(v => v.ShortUrlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ShortUrl.Visits))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
