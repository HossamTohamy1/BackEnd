namespace loxxking_backend_clean.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string NameEn, string NameAr) : IRequest<Result>;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateCategoryHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure(new Error("Error.NotFound", "Category_NotFound"));
        cat.UpdateDetails(request.NameAr, request.NameEn, cat.Slug, cat.ImageUrl);
        _context.Categories.Update(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
