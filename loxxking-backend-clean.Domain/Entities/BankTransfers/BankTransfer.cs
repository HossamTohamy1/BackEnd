using loxxking_backend_clean.Domain.Entities.Orders;

namespace loxxking_backend_clean.Domain.Entities.BankTransfers;
public class BankTransfer : BaseEntity {
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string ProofImageUrl { get; private set; } = string.Empty;
    public BankTransferStatus Status { get; private set; } = BankTransferStatus.PendingReview;
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    protected BankTransfer() { }

    public static BankTransfer Create(Guid orderId, string proofImageUrl)
    {
        return new BankTransfer
        {
            OrderId = orderId,
            ProofImageUrl = proofImageUrl,
            Status = BankTransferStatus.PendingReview,
            SubmittedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        Status = BankTransferStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Domain_BankTransfer_RejectionReasonRequired", nameof(reason));
        }

        Status = BankTransferStatus.Rejected;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
