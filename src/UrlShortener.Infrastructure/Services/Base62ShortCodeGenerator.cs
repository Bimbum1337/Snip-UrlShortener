using System.Security.Cryptography;
using UrlShortener.Application.Abstractions.Services;

namespace UrlShortener.Infrastructure.Services;

public sealed class Base62ShortCodeGenerator : IShortCodeGenerator
{
    // 0/O and 1/l/I are left out - these get read aloud and retyped.
    private const string Alphabet = "23456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

    public string Generate(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);

        // Random rather than sequential, so codes cannot be enumerated.
        return string.Create(length, Alphabet, static (span, alphabet) =>
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        });
    }
}
