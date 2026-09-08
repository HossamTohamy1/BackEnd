using loxxking_backend_clean.Domain.Entities.BankTransfers;
using loxxking_backend_clean.Domain.Entities.Notifications;


namespace loxxking_backend_clean.Application.Features.BankTransfers.Commands.UploadTransfer;

public class UploadTransferHandler : IRequestHandler<UploadTransferCommand, Result<UploadTransferResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public UploadTransferHandler(IApplicationDbContext context, IFileStorageService fileStorageService, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _localizer = localizer;
    }

    public async Task<Result<UploadTransferResponse>> Handle(UploadTransferCommand request, CancellationToken cancellationToken)
    {
        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
        if (!allowedTypes.Contains(request.ContentType))
        {
            return Result.Failure<UploadTransferResponse>(new Error("Error.Validation", "BankTransfer_OnlyImagesAllowed"));
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && (o.CustomerId == request.UserId || request.UserId == Guid.Empty), cancellationToken);

        if (order is null)
        {
            return Result.Failure<UploadTransferResponse>(new Error("Error.NotFound", "Order_NotFound"));
        }

        var existing = await _context.BankTransfers
            .AnyAsync(bt => bt.OrderId == request.OrderId, cancellationToken);

        if (existing)
        {
            return Result.Failure<UploadTransferResponse>(new Error("Error.Validation", "BankTransfer_AlreadySubmitted"));
        }

        var imageUrl = await _fileStorageService.UploadAsync(request.FileStream, request.FileName, request.ContentType, "bank-transfers", cancellationToken);

        var transfer = BankTransfer.Create(request.OrderId, imageUrl);

        _context.BankTransfers.Add(transfer);

        order.ChangePaymentStatus(PaymentStatus.PendingVerification);
        _context.Orders.Update(order);

        await _context.SaveChangesAsync(cancellationToken);

        var reviewers = await _context.Users
            .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.StoreManager)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notifMsg = _localizer.Get("Notification_BankTransferSubmitted", "A new bank transfer proof needs review.");
        foreach (var reviewerId in reviewers)
        {
            _context.Notifications.Add(Notification.Create(
                reviewerId,
                NotificationType.BankTransferSubmitted,
                notifMsg,
                order.Id
            ));
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UploadTransferResponse(transfer.Id, transfer.Status.ToString()));
    }
}
