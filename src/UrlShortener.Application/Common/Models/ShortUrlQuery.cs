namespace UrlShortener.Application.Common.Models;

public enum ShortUrlSortBy
{
    CreatedAt = 0,
    Visits = 1,
    LastVisitedAt = 2,
    Code = 3,
}

public sealed record ShortUrlQuery
{
    public const int MaxPageSize = 100;

    private readonly int _page = 1;
    private readonly int _pageSize = 20;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => value,
        };
    }

    // Matched against code, title and destination.
    public string? Search { get; init; }

    // null = any, true = active only, false = disabled only.
    public bool? IsActive { get; init; }

    public ShortUrlSortBy SortBy { get; init; } = ShortUrlSortBy.CreatedAt;

    public bool Descending { get; init; } = true;

    public int Skip => (Page - 1) * PageSize;
}
