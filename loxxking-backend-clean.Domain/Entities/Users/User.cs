using loxxking_backend_clean.Domain.Entities.Countries;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace loxxking_backend_clean.Domain.Entities.Users;

public class User : IdentityUser<Guid> {
    public string Name { get; private set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public string Phone 
    { 
        get => PhoneNumber!; 
        private set => PhoneNumber = value; 
    }

    public DateTime? LastLoginAt { get; private set; }
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    private User() { } // EF Core

    public static User Create(
        string name, string email, string phone, 
        string passwordHash, Guid countryId, 
        UserRole role, string? preferredLanguage = "en")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Domain_User_NameRequired", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Domain_User_EmailRequired", nameof(email));
        if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Domain_User_PhoneRequired", nameof(phone));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Domain_User_PasswordHashRequired", nameof(passwordHash));
        if (countryId == Guid.Empty) throw new ArgumentException("Domain_User_CountryIdRequired", nameof(countryId));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            UserName = email, // Required by Identity
            Phone = phone,
            PasswordHash = passwordHash,
            CountryId = countryId,
            Role = role,
            PreferredLanguage = preferredLanguage,
            CreatedAt = DateTime.UtcNow
        };
        
        return user;
    }

    public void UpdateProfile(string name, string email, string phone, Guid countryId, string? preferredLanguage)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Domain_User_NameRequired", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Domain_User_EmailRequired", nameof(email));
        if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Domain_User_PhoneRequired", nameof(phone));
        if (countryId == Guid.Empty) throw new ArgumentException("Domain_User_CountryIdRequired", nameof(countryId));

        Name = name;
        Email = email;
        UserName = email; // Keep in sync
        Phone = phone;
        CountryId = countryId;
        PreferredLanguage = preferredLanguage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Domain_User_PasswordHashRequired", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRefreshToken(string refreshTokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
    }

    public void UpdateLastLogin(DateTime loginAt)
    {
        LastLoginAt = loginAt;
    }
}
