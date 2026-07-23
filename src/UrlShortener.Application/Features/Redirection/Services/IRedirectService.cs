using UrlShortener.Application.Features.Redirection.Dtos;
using UrlShortener.Domain.Common;

namespace UrlShortener.Application.Features.Redirection.Services;

public interface IRedirectService
{
    Task<Result<RedirectTarget>> ResolveAsync(
        string code,
        VisitContext context,
        CancellationToken cancellationToken = default);

    // Same lookup, but without counting a visit.
    Task<Result<RedirectTarget>> PeekAsync(string code, CancellationToken cancellationToken = default);
}
