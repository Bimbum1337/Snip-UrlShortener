using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Features.ShortUrls.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Features.ShortUrls.Mapping;

public static class ShortUrlMapper
{
    public static ShortUrlResponse ToResponse(this ShortUrl entity, IShortLinkBuilder linkBuilder, DateTimeOffset nowUtc)
        => new(
            entity.Id,
            entity.Code,
            linkBuilder.BuildAbsoluteUrl(entity.Code),
            entity.Destination,
            entity.Title,
            entity.IsCustomAlias,
            entity.IsActive,
            entity.IsExpired(nowUtc),
            entity.VisitCount,
            entity.CreatedAtUtc,
            entity.ExpiresAtUtc,
            entity.LastVisitedAtUtc);

    public static IReadOnlyList<ShortUrlResponse> ToResponses(
        this IEnumerable<ShortUrl> entities,
        IShortLinkBuilder linkBuilder,
        DateTimeOffset nowUtc)
        => entities.Select(e => e.ToResponse(linkBuilder, nowUtc)).ToList();
}
