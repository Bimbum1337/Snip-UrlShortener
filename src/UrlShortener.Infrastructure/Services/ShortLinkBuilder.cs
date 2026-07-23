using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Options;

namespace UrlShortener.Infrastructure.Services;

public sealed class ShortLinkBuilder(IOptions<ShortenerOptions> options) : IShortLinkBuilder
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

    public string BuildAbsoluteUrl(string code)
        => string.IsNullOrEmpty(code) ? _baseUrl : $"{_baseUrl}/{code}";
}
