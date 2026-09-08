using loxxking_backend_clean.Domain.Entities.Users;

namespace loxxking_backend_clean.Application.Common.Interfaces;

public interface IJwtProvider
{
    string Generate(User user);
}
