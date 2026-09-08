using System;
using System.Collections.Generic;

namespace loxxking_backend_clean.Domain.ValueObjects;

public class RatingScore : IEquatable<RatingScore>
{
    public int Value { get; }

    private RatingScore(int value)
    {
        if (value < 1 || value > 5)
        {
            throw new ArgumentException("Domain_RatingScore_Range", nameof(value));
        }
        
        Value = value;
    }

    public static RatingScore FromInt(int value)
    {
        return new RatingScore(value);
    }

    public bool Equals(RatingScore? other)
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
        return Equals((RatingScore)obj);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(RatingScore? left, RatingScore? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RatingScore? left, RatingScore? right)
    {
        return !Equals(left, right);
    }
}
