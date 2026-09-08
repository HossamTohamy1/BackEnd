namespace loxxking_backend_clean.Domain.ValueObjects;

public class Percentage : IEquatable<Percentage>
{
    public decimal Value { get; private set; }

    protected Percentage() { }

    private Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Domain_Percentage_Range", nameof(value));
        Value = value;
    }

    public static Percentage FromDecimal(decimal value) => new Percentage(value);

    internal static Percentage UnsafeFromDatabase(decimal value) => new Percentage { Value = value };

    public static Percentage Zero => new Percentage(0);

    public bool Equals(Percentage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Percentage)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Percentage? left, Percentage? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Percentage? left, Percentage? right)
    {
        return !Equals(left, right);
    }
}
