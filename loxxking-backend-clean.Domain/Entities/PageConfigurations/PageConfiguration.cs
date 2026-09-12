using System;

namespace loxxking_backend_clean.Domain.Entities.PageConfigurations;

public class PageConfiguration
{
    public string Key { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
