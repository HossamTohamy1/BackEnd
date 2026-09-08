using Microsoft.AspNetCore.Identity;
using loxxking_backend_clean.Domain.Entities.Users;

namespace loxxking_backend_clean.Application.Features.Users.Commands.ResetStaffPassword;

public record ResetStaffPasswordCommand(Guid StaffId, string NewPassword) : IRequest<Result>;

public class ResetStaffPasswordHandler : IRequestHandler<ResetStaffPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public ResetStaffPasswordHandler(IApplicationDbContext context, ICurrentUserService currentUserService, UserManager<User> userManager) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result> Handle(ResetStaffPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StaffId, cancellationToken);
        if (user == null) return Result.Failure(new Error("Error.NotFound", "User_StaffNotFound"));

        if (user.Id == _currentUserService.UserId)
            return Result.Failure(new Error("Validation", "User_UseChangePasswordEndpoint"));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("Validation", errors));
        }
        
        return Result.Success();
    }
}
