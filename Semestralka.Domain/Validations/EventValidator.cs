using Semestralka.Domain.Exceptions;

namespace Semestralka.Domain.Validations;

public static class EventValidator
{
    public static void ValidateCreate(
        string title,
        DateTimeOffset start,
        DateTimeOffset end,
        string? description,
        string? location
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Název události je povinný.");

        if (title.Length > 100)
            throw new DomainValidationException(
                "Název události může mít maximálně 100 znaků."
            );

        if (description?.Length > 500)
            throw new DomainValidationException(
                "Popis může mít maximálně 500 znaků."
            );

        if (location?.Length > 200)
            throw new DomainValidationException(
                "Místo může mít maximálně 200 znaků."
            );

        if (end < start)
            throw new DomainValidationException(
                "Čas konce události nesmí být dříve než čas začátku."
            );
    }
}
