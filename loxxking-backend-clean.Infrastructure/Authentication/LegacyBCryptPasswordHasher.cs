using loxxking_backend_clean.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace loxxking_backend_clean.Infrastructure.Authentication;

public class LegacyBCryptPasswordHasher : IPasswordHasher<User>
{
    private readonly PasswordHasher<User> _identityPasswordHasher;
    

    private readonly bool _useBCryptForNewHashes = false;

    public LegacyBCryptPasswordHasher()
    {
        _identityPasswordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        if (_useBCryptForNewHashes)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        return _identityPasswordHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith("$2a$") || hashedPassword.StartsWith("$2b$") || hashedPassword.StartsWith("$2y$"))
        {
            var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            if (!isValid)
            {
                return PasswordVerificationResult.Failed;
            }

            if (_useBCryptForNewHashes)
            {
                return PasswordVerificationResult.Success;
            }
            
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        return _identityPasswordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }
}
