namespace AuthService.Domain.ValueObjects;

public class HashedPassword
{
    public string Value { get; }

    private HashedPassword(string value)
    {
        Value = value;
    }

    public static HashedPassword Create(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password hash cannot be empty", nameof(hashedPassword));

        return new HashedPassword(hashedPassword);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is HashedPassword other)
            return Value == other.Value;

        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}