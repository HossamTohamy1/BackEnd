namespace loxxking_backend_clean.Application.Features.Products.Commands.UploadProductImage;

public record UploadProductImageCommand(Guid ProductId, Stream FileStream, string FileName, string ContentType) : IRequest<Result<string>>;

public class UploadProductImageHandler : IRequestHandler<UploadProductImageCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    public UploadProductImageHandler(IApplicationDbContext context, IFileStorageService fileStorage) { _context = context; _fileStorage = fileStorage; }

    public async Task<Result<string>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure<string>(new Error("Error.NotFound", "Product_NotFound"));
        
        var url = await _fileStorage.UploadAsync(request.FileStream, request.FileName, request.ContentType, "products", cancellationToken);
        var imagesList = product.Images?.ToList() ?? new List<string>();
        imagesList.Add(url);
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
        return Result.Success(url);
    }
}
