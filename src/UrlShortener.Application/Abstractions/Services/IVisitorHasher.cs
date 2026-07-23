namespace UrlShortener.Application.Abstractions.Services;

public interface IVisitorHasher
{
    string? Hash(string? ipAddress);
}
