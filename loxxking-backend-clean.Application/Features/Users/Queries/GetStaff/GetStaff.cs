namespace loxxking_backend_clean.Application.Features.Users.Queries.GetStaff;

public record GetStaffQuery(UserRole? Role) : IRequest<Result<List<StaffResponse>>>;
public record StaffResponse(Guid Id, string Name, string Email, string Phone, string Role, bool IsActive);

public class GetStaffHandler : IRequestHandler<GetStaffQuery, Result<List<StaffResponse>>>
{
    private readonly IApplicationDbContext _context;
    public GetStaffHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<List<StaffResponse>>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.Where(u => u.Role != UserRole.Customer);
        if (request.Role.HasValue) query = query.Where(u => u.Role == request.Role.Value);
        var staff = await query.Select(u => new StaffResponse(u.Id, u.Name, u.Email, u.PhoneNumber!, u.Role.ToString(), u.IsActive)).ToListAsync(cancellationToken);
        return Result.Success(staff);
    }
}
