namespace loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;

public record LoginUserQuery(string Email, string Password) : IRequest<Result<LoginUserResponse>>;

public record LoginUserResponse(string Token, Guid UserId, string Role);
