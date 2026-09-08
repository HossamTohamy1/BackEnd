using loxxking_backend_clean.Domain.Entities.Reviews;
using loxxking_backend_clean.Domain.ValueObjects;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Reviews;

public class ReviewSeeder : IDataSeeder
{
    public int Order => 9;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<ReviewSeeder>>();

        var shirt = context.Products.FirstOrDefault(p => p.Slug == "oxford-cotton-shirt");
        var loafers = context.Products.FirstOrDefault(p => p.Slug == "italian-leather-loafers");
        var customer = context.CustomerUser;

        if (shirt != null && customer != null)
        {
            var userReviewExists = await dbContext.Reviews.AnyAsync(
                r => r.ProductId == shirt.Id && r.UserId == customer.Id, 
                cancellationToken);

            if (!userReviewExists)
            {
                var review = Review.Create(
                    productId: shirt.Id,
                    userId: customer.Id,
                    guestName: null,
                    rating: RatingScore.FromInt(5),
                    comment: "خامة ممتازة جداً والمقاس مضبوط تماماً، جودة فاخرة تستحق السعر.");
                
                review.Approve();

                await dbContext.Reviews.AddAsync(review, cancellationToken);
                logger.LogInformation("Seeded customer review for product '{ShirtSlug}'.", shirt.Slug);
            }
        }

        if (loafers != null)
        {
            const string guestName = "م. كريم حسام";
            var guestReviewExists = await dbContext.Reviews.AnyAsync(
                r => r.ProductId == loafers.Id && r.GuestName == guestName, 
                cancellationToken);

            if (!guestReviewExists)
            {
                var guestReview = Review.Create(
                    productId: loafers.Id,
                    userId: null,
                    guestName: guestName,
                    rating: RatingScore.FromInt(5),
                    comment: "حذاء مريح جداً في المشي والجلد طبيعي أصلي وفخم.");

                guestReview.Approve();

                await dbContext.Reviews.AddAsync(guestReview, cancellationToken);
                logger.LogInformation("Seeded guest review for product '{LoafersSlug}'.", loafers.Slug);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
