namespace loxxking_backend_clean.Application.Features.Users.Commands.ToggleStaffStatus;

public record ToggleStaffStatusCommand(Guid Id) : IRequest<Result>;

public class ToggleStaffStatusHandler : IRequestHandler<ToggleStaffStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleStaffStatusHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ToggleStaffStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));

        if (user.Id == _currentUserService.UserId)
            return Result.Failure(new Error("Validation", "User_CannotDeactivateOwnAccount"));

        user.IsActive = !user.IsActive;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
