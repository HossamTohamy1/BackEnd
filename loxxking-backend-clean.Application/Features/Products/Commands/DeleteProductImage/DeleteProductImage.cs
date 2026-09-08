namespace loxxking_backend_clean.Application.Features.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, string Url) : IRequest<Result>;

public class DeleteProductImageHandler : IRequestHandler<DeleteProductImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteProductImageHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure(new Error("Error.NotFound", "Product_NotFound"));
        var imagesList = product.Images?.ToList() ?? new List<string>();
        imagesList.Remove(request.Url);
        product.UpdateDetails(
            categoryId: product.CategoryId,
            nameAr: product.NameAr,
            nameEn: product.NameEn,
            description: product.Description,
            slug: product.Slug,
            basePrice: product.BasePrice,
            originalPrice: product.OriginalPrice,
            images: imagesList,
            sizes: product.Sizes,
            colors: product.Colors,
            features: product.Features,
            shippingPolicy: product.ShippingPolicy,
            returnPolicy: product.ReturnPolicy,
            sizeChartJson: product.SizeChartJson,
            isNew: product.IsNew,
            isBestSeller: product.IsBestSeller,
            badge: product.Badge
        );
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
