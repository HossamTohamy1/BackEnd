using loxxking_backend_clean.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace loxxking_backend_clean.Application.Features.Users.Commands.CreateStoreManager;

public record CreateStoreManagerCommand(string Name, string Email, string Phone, string Password) : IRequest<Result<Guid>>;

public class CreateStoreManagerHandler : IRequestHandler<CreateStoreManagerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CreateStoreManagerHandler(IApplicationDbContext context, UserManager<User> userManager) 
    { 
        _context = context; 
        _userManager = userManager;
    }

    public async Task<Result<Guid>> Handle(CreateStoreManagerCommand request, CancellationToken cancellationToken)
    {
        var defaultCountry = await _context.Countries.FirstOrDefaultAsync(c => c.IsDefault, cancellationToken);
        if (defaultCountry == null) return Result.Failure<Guid>(new Error("Validation", "User_NoDefaultCountry"));

        var user = User.Create(
            request.Name, 
            request.Email, 
            request.Phone, 
            "#PENDING_HASH#", 
            defaultCountry.Id, 
            UserRole.StoreManager);

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(new Error("Validation", errors));
        }

        return Result.Success(user.Id);
    }
}
