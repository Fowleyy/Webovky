using System.Text.RegularExpressions;
using Semestralka.Domain.Exceptions;

namespace Semestralka.Domain.Validations;

public static class CalendarValidator
{
    public static void ValidateCreate(
        Guid ownerId,
        string? color,
        string visibility
    )
    {
        if (ownerId == Guid.Empty)
            throw new DomainValidationException("Vlastník kalendáře je povinný.");

        ValidateVisibility(visibility);
        ValidateColor(color);
    }

    private static void ValidateVisibility(string visibility)
    {
        if (visibility != "public" && visibility != "private")
            throw new DomainValidationException(
                "Viditelnost musí být 'public' nebo 'private'."
            );
    }

    private static void ValidateColor(string? color)
    {
        if (color == null) return;

        if (!Regex.IsMatch(color, "^#([0-9a-fA-F]{6})$"))
            throw new DomainValidationException("Barva musí být ve formátu HEX.");
    }
}
