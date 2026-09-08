using loxxking_backend_clean.Domain.Entities.BankTransfers;
using loxxking_backend_clean.Domain.Entities.Reviews;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Domain.ValueObjects;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyTier4RuntimeBehavior
{
    [Fact]
    public void Verify_RatingScore_ValueObject()
    {
        var r1 = RatingScore.FromInt(1);
        var r5 = RatingScore.FromInt(5);
        Assert.Equal(1, r1.Value);
        Assert.Equal(5, r5.Value);

        Assert.Throws<ArgumentException>(() => RatingScore.FromInt(0));
        Assert.Throws<ArgumentException>(() => RatingScore.FromInt(6));

        var r3a = RatingScore.FromInt(3);
        var r3b = RatingScore.FromInt(3);
        Assert.True(r3a.Equals(r3b));
        Assert.True(r3a == r3b);
    }

    [Fact]
    public void Verify_Review_Invariants_And_StateTransitions()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rating = RatingScore.FromInt(5);

        Assert.Throws<ArgumentException>(() => Review.Create(productId, null, null, rating, "Great"));
        Assert.Throws<ArgumentException>(() => Review.Create(productId, null, " ", rating, "Great"));
        
        var guestReview = Review.Create(productId, null, "Guest Bob", rating, "Awesome!");
        Assert.Equal("Guest Bob", guestReview.GuestName);
        Assert.Null(guestReview.UserId);

        var userReview = Review.Create(productId, userId, null, rating, "Awesome!");
        Assert.Equal(userId, userReview.UserId);
        Assert.Null(userReview.GuestName);

        var userWithGuestName = Review.Create(productId, userId, "Ignored Guest", rating, "Awesome!");
        Assert.Null(userWithGuestName.GuestName);

        Assert.Equal(ReviewStatus.Pending, guestReview.Status);
        
        guestReview.Approve();
        Assert.Equal(ReviewStatus.Approved, guestReview.Status);
        
        guestReview.Hide();
        Assert.Equal(ReviewStatus.Hidden, guestReview.Status);
        
        guestReview.Reject();
        Assert.Equal(ReviewStatus.Rejected, guestReview.Status);
    }

    [Fact]
    public void Verify_BankTransfer_Invariants_And_StateTransitions()
    {
        var orderId = Guid.NewGuid();
        var proofUrl = "https://example.com/proof.jpg";

        var transfer = BankTransfer.Create(orderId, proofUrl);
        Assert.Equal(BankTransferStatus.PendingReview, transfer.Status);
        Assert.Null(transfer.ReviewedAt);
        Assert.Null(transfer.RejectionReason);

        Assert.Throws<ArgumentException>(() => transfer.Reject(""));
        Assert.Throws<ArgumentException>(() => transfer.Reject("   "));
        Assert.Throws<ArgumentException>(() => transfer.Reject(null!));

        transfer.Reject("Image is blurry");
        Assert.Equal(BankTransferStatus.Rejected, transfer.Status);
        Assert.Equal("Image is blurry", transfer.RejectionReason);
        Assert.NotNull(transfer.ReviewedAt);

        transfer.Approve();
        Assert.Equal(BankTransferStatus.Approved, transfer.Status);
        Assert.Null(transfer.RejectionReason); // Reason cleared!
        Assert.NotNull(transfer.ReviewedAt);
    }
}
