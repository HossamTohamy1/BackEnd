namespace loxxking_backend_clean.Application.Features.Reviews.Helpers;

public static class ReviewSyncHelper
{
    public static async Task SyncProductRatingAsync(Guid productId, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null) return;

        var existingReviews = await context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);

        var localReviews = context.Reviews.Local
            .Where(r => r.ProductId == productId)
            .ToList();

        var allReviews = existingReviews.Union(localReviews).ToList();

        var approvedReviews = allReviews.Where(r => r.Status == ReviewStatus.Approved).ToList();

        if (approvedReviews.Any())
        {
            product.UpdateRating(approvedReviews.Average(r => r.Rating.Value), approvedReviews.Count);
        }
        else
        {
            product.UpdateRating(0, 0);
        }

    }
}
