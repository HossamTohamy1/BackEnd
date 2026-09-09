using System;
using Microsoft.Extensions.Caching.Memory;
using loxxking_backend_clean.Application.Common.Interfaces;

namespace loxxking_backend_clean.Infrastructure.Services;

public class MemoryCacheSsoJtiValidator : ISsoJtiValidator
{
    private readonly IMemoryCache _cache;
    private readonly object _syncLock = new();

    public MemoryCacheSsoJtiValidator(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryConsume(string jti, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(jti)) return false;
        var key = $"sso_consumed_jti_{jti}";

        lock (_syncLock)
        {
            if (_cache.TryGetValue(key, out _))
            {
                return false; 
            }

            _cache.Set(key, true, duration);
            return true;
        }
    }
}
