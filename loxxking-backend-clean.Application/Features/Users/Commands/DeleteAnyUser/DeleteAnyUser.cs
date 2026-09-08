namespace loxxking_backend_clean.Application.Features.Users.Commands.DeleteAnyUser;

public record DeleteAnyUserCommand(Guid TargetId) : IRequest<Result>;

public class DeleteAnyUserHandler : IRequestHandler<DeleteAnyUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAnyUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteAnyUserCommand request, CancellationToken cancellationToken)
    {
        var target = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetId, cancellationToken);
        if (target == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));

        if (target.Id == _currentUserService.UserId)
            return Result.Failure(new Error("Validation", "User_CannotDeleteOwnAccount"));

        if (target.Role == loxxking_backend_clean.Domain.Enums.UserRole.Admin)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
            if (currentUser?.Role != loxxking_backend_clean.Domain.Enums.UserRole.Admin)
                return Result.Failure(new Error("Validation", "User_OnlyAdminCanDeleteAdmin"));
        }

        _context.Users.Remove(target);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
