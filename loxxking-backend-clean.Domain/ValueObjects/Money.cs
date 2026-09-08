namespace loxxking_backend_clean.Domain.ValueObjects;

public class Money : IEquatable<Money>
{
    public decimal Value { get; private set; }

    protected Money() { }

    private Money(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Domain_Money_CannotBeNegative", nameof(value));
        Value = value;
    }

    public static Money FromDecimal(decimal value) => new Money(value);

    internal static Money UnsafeFromDatabase(decimal value) => new Money { Value = value };

    public static Money operator +(Money left, Money right) => new Money(left.Value + right.Value);
    
    public static Money operator -(Money left, Money right) 
    {
        return new Money(left.Value - right.Value);
    }
    
    public static Money operator *(Money left, decimal multiplier) => new Money(left.Value * multiplier);
    
    public static Money operator /(Money left, decimal divisor)
    {
        if (divisor == 0)
            throw new ArgumentException("Domain_Money_CannotDivideByZero", nameof(divisor));
            
        return new Money(left.Value / divisor);
    }

    public static Money Zero => new Money(0);

    public bool Equals(Money? other)
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
        return Equals((Money)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Money? left, Money? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !Equals(left, right);
    }
}
