namespace UrlShortener.Application.Abstractions.Services;

public interface IShortLinkBuilder
{
    string BuildAbsoluteUrl(string code);
}
