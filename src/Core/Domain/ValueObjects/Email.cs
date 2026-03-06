using Domain.ValueObjects;

namespace Domain.ValueObjects;

/// <summary>
/// Sample Value Object: Email address.
///
/// Demonstrates key Value Object patterns:
///   1. Immutability  — properties are init-only, set only via factory method
///   2. Self-validation — Create() enforces the business rule before accepting a value
///   3. Structural equality — C# records compare by value automatically
///   4. No public constructor — construction always goes through Create() to guarantee validity
///
/// USAGE:
///   var email = Email.Create("user@example.com");       // returns Ok
///   var bad   = Email.Create("not-an-email");            // throws ArgumentException
/// </summary>
public record Email : IValueObject
{
    /// <summary>The normalized (lowercased, trimmed) email string.</summary>
    public string Value { get; }

    // Private constructor forces all callers to go through the factory method,
    // ensuring that an Email can never exist in an invalid state.
    private Email(string value) => Value = value;

    /// <summary>
    /// Factory method that validates and creates an Email value object.
    /// Throws <see cref="ArgumentException"/> if the format is invalid.
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        email = email.Trim().ToLowerInvariant();

        // A minimal format check — replace with a more robust validator if needed.
        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));

        return new Email(email);
    }

    // Implicit conversion lets you use an Email anywhere a string is expected.
    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
