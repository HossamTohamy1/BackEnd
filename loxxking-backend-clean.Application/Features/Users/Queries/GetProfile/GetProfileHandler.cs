namespace loxxking_backend_clean.Application.Features.Users.Queries.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<GetProfileResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => new GetProfileResponse(u.Id, u.Name, u.Email, u.PhoneNumber!, u.CountryId, u.Country.Name, u.Role.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetProfileResponse>(new Error("Error.NotFound", "User_NotFound"));
        }

        return Result.Success(user);
    }
}
