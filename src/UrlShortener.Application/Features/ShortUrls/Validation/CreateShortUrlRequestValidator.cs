using FluentValidation;
using UrlShortener.Application.Features.ShortUrls.Dtos;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Features.ShortUrls.Validation;

// Shape checks only, to reject junk before touching the database. The real
// rules live in ShortCode and DestinationUrl.
public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("A destination URL is required.")
            .MaximumLength(DestinationUrl.MaxLength);

        RuleFor(x => x.CustomAlias)
            .Length(ShortCode.MinLength, ShortCode.MaxLength)
            .Matches("^[A-Za-z0-9_-]+$")
                .WithMessage("An alias may only contain letters, digits, hyphens and underscores.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomAlias));

        RuleFor(x => x.Title)
            .MaximumLength(200);
    }
}

public sealed class UpdateShortUrlRequestValidator : AbstractValidator<UpdateShortUrlRequest>
{
    public UpdateShortUrlRequestValidator()
    {
        RuleFor(x => x.Destination)
            .NotEmpty()
            .MaximumLength(DestinationUrl.MaxLength)
            .When(x => x.Destination is not null);

        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x)
            .Must(x => !(x.ClearExpiry && x.ExpiresAtUtc is not null))
            .WithMessage("Cannot set and clear the expiry in the same request.");
    }
}
