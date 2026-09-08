namespace loxxking_backend_clean.Application.Features.Products.Queries.GetProductDetail;

public record GetProductDetailQuery(Guid Id) : IRequest<Result<object>>;

public class GetProductDetailHandler : IRequestHandler<GetProductDetailQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;
    public GetProductDetailHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<object>> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Where(p => p.Id == request.Id).Select(p => new { p.Id, p.NameEn, p.NameAr, p.Description, p.BasePrice, p.Images }).FirstOrDefaultAsync(cancellationToken);
        if (product == null) return Result.Failure<object>(new Error("Error.NotFound", "Product_NotFound"));
        return Result.Success<object>(product);
    }
}
