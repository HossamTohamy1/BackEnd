namespace loxxking_backend_clean.Domain.ValueObjects;

public class DateRange : IEquatable<DateRange>
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    protected DateRange() { }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Domain_DateRange_StartDateAfterEndDate", nameof(startDate));
        
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate) => new DateRange(startDate, endDate);

    public bool IsActive(DateTime date) => date >= StartDate && date <= EndDate;

    public bool Equals(DateRange? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DateRange)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartDate, EndDate);
    }

    public static bool operator ==(DateRange? left, DateRange? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DateRange? left, DateRange? right)
    {
        return !Equals(left, right);
    }
}
