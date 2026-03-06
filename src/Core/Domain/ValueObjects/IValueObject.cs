namespace Domain.ValueObjects;

/// <summary>
/// Marker interface for Value Objects.
///
/// KEY CONCEPTS:
/// - A Value Object is defined entirely by its attributes, not by an identity.
/// - Two value objects with the same attributes are considered equal.
/// - Value Objects are always immutable — once created they cannot change.
///   To "change" a value object, you replace it with a new instance.
///
/// COMPARISON vs ENTITY:
/// - Entity:       "Are you the same customer?" → compared by Id
/// - Value Object: "Is this the same email address?" → compared by value
///
/// WHEN TO USE:
/// Use Value Objects for concepts that describe characteristics of something:
///   Email, Money, Address, PhoneNumber, DateRange, Coordinates.
///
/// IMPLEMENTATION GUIDE:
/// Since C# records automatically implement structural equality, prefer using
/// 'record' types for value objects:
///
///   public record Email : IValueObject
///   {
///       public string Value { get; }
///       private Email(string value) => Value = value;
///       public static Email Create(string value) { /* validate */ return new Email(value); }
///   }
/// </summary>
public interface IValueObject { }
