using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Options;

namespace UrlShortener.Infrastructure.Services;

public sealed class SaltedVisitorHasher(IOptions<ShortenerOptions> options) : IVisitorHasher
{
    private readonly byte[] _salt = Encoding.UTF8.GetBytes(options.Value.VisitorHashSalt);

    public string? Hash(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        var input = Encoding.UTF8.GetBytes(ipAddress);
        var buffer = new byte[_salt.Length + input.Length];

        _salt.CopyTo(buffer, 0);
        input.CopyTo(buffer, _salt.Length);

        return Convert.ToHexString(SHA256.HashData(buffer))[..32];
    }
}
