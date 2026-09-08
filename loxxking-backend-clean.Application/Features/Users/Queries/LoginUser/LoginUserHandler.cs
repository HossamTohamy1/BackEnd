using loxxking_backend_clean.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserQuery, Result<LoginUserResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtProvider _jwtProvider;
    private readonly UserManager<User> _userManager;

    public LoginUserHandler(IApplicationDbContext context, IJwtProvider jwtProvider, UserManager<User> userManager)
    {
        _context = context;
        _jwtProvider = jwtProvider;
        _userManager = userManager;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginUserResponse>(new Error("Error.Unauthorized", "Auth_InvalidCredentials"));
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result.Failure<LoginUserResponse>(new Error("Error.Unauthorized", "Auth_InvalidCredentials"));
        }

        if (user.PasswordHash != null)
        {
            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
            }
        }

        user.UpdateLastLogin(DateTime.UtcNow);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtProvider.Generate(user);

        return Result.Success(new LoginUserResponse(token, user.Id, user.Role.ToString()));
    }
}
