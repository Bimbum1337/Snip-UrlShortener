using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

public sealed class UrlVisitConfiguration : IEntityTypeConfiguration<UrlVisit>
{
    public void Configure(EntityTypeBuilder<UrlVisit> builder)
    {
        builder.ToTable("UrlVisits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VisitedAtUtc).IsRequired();
        builder.Property(x => x.Referer).HasMaxLength(512);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.IpHash).HasMaxLength(64);

        // Every analytics query filters by link, then by date.
        builder.HasIndex(x => new { x.ShortUrlId, x.VisitedAtUtc });
    }
}
