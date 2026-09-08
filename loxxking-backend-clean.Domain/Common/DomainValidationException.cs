namespace loxxking_backend_clean.Domain.Common;

public class DomainValidationException : ArgumentException
{
    public string ResourceKey { get; }
    public object[] Args { get; }

    public DomainValidationException(string resourceKey, params object[] args)
        : base(resourceKey)
    {
        ResourceKey = resourceKey;
        Args = args ?? Array.Empty<object>();
    }

    public DomainValidationException(string resourceKey, string? paramName, params object[] args)
        : base(resourceKey, paramName)
    {
        ResourceKey = resourceKey;
        Args = args ?? Array.Empty<object>();
    }
}
