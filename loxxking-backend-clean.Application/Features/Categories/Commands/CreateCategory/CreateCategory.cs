using loxxking_backend_clean.Domain.Entities.Categories;

namespace loxxking_backend_clean.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string NameEn, string NameAr) : IRequest<Result<Guid>>;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateCategoryHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = request.NameEn.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString().Substring(0, 8);
        var cat = Category.Create(request.NameAr, request.NameEn, slug, string.Empty);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(cat.Id);
    }
}
