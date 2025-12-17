using System.Text.RegularExpressions;
using Semestralka.Domain.Exceptions;

namespace Semestralka.Domain.Validations;

public static class UserValidator
{
    public static void ValidateRegister(
        string email,
        string password,
        string fullName,
        string timeZone
    )
    {
        ValidateEmail(email);
        ValidatePassword(password);
        ValidateFullName(fullName);

        if (string.IsNullOrWhiteSpace(timeZone))
            throw new DomainValidationException("Časové pásmo je povinné.");
    }

    public static void ValidateLogin(string email, string password)
    {
        ValidateEmail(email);

        if (string.IsNullOrWhiteSpace(password))
            throw new DomainValidationException("Heslo je povinné.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email je povinný.");

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainValidationException("Email nemá platný formát.");
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 6)
            throw new DomainValidationException("Heslo musí mít alespoň 6 znaků.");
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainValidationException("Jméno je povinné.");

        if (fullName.Length > 100)
            throw new DomainValidationException("Jméno je příliš dlouhé.");
    }
}
