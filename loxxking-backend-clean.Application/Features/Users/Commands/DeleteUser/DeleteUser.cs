namespace loxxking_backend_clean.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid TargetId) : IRequest<Result>;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
    { 
        _context = context; 
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var target = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetId, cancellationToken);
        if (target == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));
        
        if (target.Id == _currentUserService.UserId) 
            return Result.Failure(new Error("Validation", "User_CannotDeleteOwnAccount"));
            
        if (target.Role == UserRole.Admin) 
            return Result.Failure(new Error("Validation", "User_CannotDeleteAdmin"));

        _context.Users.Remove(target);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
