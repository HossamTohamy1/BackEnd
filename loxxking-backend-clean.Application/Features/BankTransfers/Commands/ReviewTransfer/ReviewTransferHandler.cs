using loxxking_backend_clean.Domain.Entities.Notifications;


namespace loxxking_backend_clean.Application.Features.BankTransfers.Commands.ReviewTransfer;

public class ReviewTransferHandler : IRequestHandler<ReviewTransferCommand, Result<ReviewTransferResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public ReviewTransferHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<ReviewTransferResponse>> Handle(ReviewTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.BankTransfers
            .Include(bt => bt.Order)
            .FirstOrDefaultAsync(bt => bt.Id == request.Id, cancellationToken);

        if (transfer is null)
        {
            return Result.Failure<ReviewTransferResponse>(new Error("Error.NotFound", "BankTransfer_NotFound"));
        }

        if (!request.Approved && string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            return Result.Failure<ReviewTransferResponse>(new Error("Error.Validation", "BankTransfer_RejectionReasonRequired"));
        }

        if (request.Approved)
        {
            transfer.Approve();
        }
        else
        {
            transfer.Reject(request.RejectionReason!);
        }
        _context.BankTransfers.Update(transfer);

        transfer.Order.ChangePaymentStatus(request.Approved ? PaymentStatus.Paid : PaymentStatus.Rejected);
        _context.Orders.Update(transfer.Order);

        if (transfer.Order.CustomerId.HasValue)
        {
            var notifMsg = request.Approved
                ? _localizer.Get("Notification_BankTransferApproved", "Your bank transfer was approved. Your order is now confirmed.")
                : _localizer.Get("Notification_BankTransferRejected", $"Your bank transfer was rejected: {request.RejectionReason}", request.RejectionReason ?? "");

            _context.Notifications.Add(Notification.Create(
                transfer.Order.CustomerId.Value,
                NotificationType.BankTransferReviewed,
                notifMsg,
                transfer.OrderId
            ));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReviewTransferResponse(transfer.Id, transfer.Status.ToString()));
    }
}
