namespace loxxking_backend_clean.Application.Features.Users.Queries.GetProfile;

public record GetProfileQuery(Guid UserId) : IRequest<Result<GetProfileResponse>>;

public record GetProfileResponse(Guid Id, string Name, string Email, string Phone, Guid CountryId, string CountryName, string Role);
