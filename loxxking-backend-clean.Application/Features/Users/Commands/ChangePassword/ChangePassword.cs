namespace loxxking_backend_clean.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest<Result>;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<loxxking_backend_clean.Domain.Entities.Users.User> _userManager;

    public ChangePasswordHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        Microsoft.AspNetCore.Identity.UserManager<loxxking_backend_clean.Domain.Entities.Users.User> userManager) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
        if (user == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));
        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("Validation", errors));
        }
        return Result.Success();
    }
}
