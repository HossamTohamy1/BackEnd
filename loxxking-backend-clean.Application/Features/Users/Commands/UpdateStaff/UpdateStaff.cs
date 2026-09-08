namespace loxxking_backend_clean.Application.Features.Users.Commands.UpdateStaff;

public record UpdateStaffCommand(Guid Id, string Name, string Phone) : IRequest<Result>;

public class UpdateStaffHandler : IRequestHandler<UpdateStaffCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateStaffHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));
        user.UpdateProfile(request.Name, user.Email, request.Phone, user.CountryId, user.PreferredLanguage);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
