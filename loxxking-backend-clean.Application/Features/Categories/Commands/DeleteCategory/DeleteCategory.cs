namespace loxxking_backend_clean.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteCategoryHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure(new Error("Error.NotFound", "Category_NotFound"));
        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
