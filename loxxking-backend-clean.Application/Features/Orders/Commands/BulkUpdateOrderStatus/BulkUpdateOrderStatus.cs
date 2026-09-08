using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.Orders;

namespace loxxking_backend_clean.Application.Features.Orders.Commands.BulkUpdateOrderStatus;

public record BulkUpdateOrderStatusCommand(List<Guid> OrderIds, OrderStatus NewStatus, Guid UserId) : IRequest<Result<BulkUpdateOrderStatusResponse>>;

public record BulkUpdateOrderStatusResponse(int UpdatedCount, List<Guid> UpdatedOrderIds);

public class BulkUpdateOrderStatusValidator : AbstractValidator<BulkUpdateOrderStatusCommand>
{
    public BulkUpdateOrderStatusValidator(Microsoft.Extensions.Localization.IStringLocalizer<loxxking_backend_clean.Shared.Resources.SharedResource> localizer)
    {
        RuleFor(x => x.OrderIds)
            .NotEmpty().WithMessage(localizer["Order_OrderIdsRequired"]);

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage(localizer["Validation_InvalidStatus"]);
    }
}

public class BulkUpdateOrderStatusHandler : IRequestHandler<BulkUpdateOrderStatusCommand, Result<BulkUpdateOrderStatusResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public BulkUpdateOrderStatusHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<BulkUpdateOrderStatusResponse>> Handle(BulkUpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Where(o => request.OrderIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var foundIds = orders.Select(o => o.Id).ToList();
        var missingIds = request.OrderIds.Except(foundIds).ToList();

        if (missingIds.Any())
        {
            var missingIdsString = string.Join(", ", missingIds);
            var errorMsg = _localizer.Get("Order_BulkIdsNotFound", $"The following Order IDs were not found or are deleted: {missingIdsString}", missingIdsString);
            return Result.Failure<BulkUpdateOrderStatusResponse>(
                new Error("Error.NotFound", errorMsg));
        }

        foreach (var order in orders)
        {
            var oldStatus = order.Status.ToString();
            order.ChangeStatus(request.NewStatus);

            _context.OrderEditLogs.Add(new OrderEditLog
            {
                OrderId = order.Id,
                EditedBy = request.UserId,
                FieldName = "Status",
                OldValue = oldStatus,
                NewValue = request.NewStatus.ToString(),
                EditedAt = DateTime.UtcNow
            });

            _context.Orders.Update(order);

            if (order.CustomerId.HasValue)
            {
                var notifMsg = _localizer.Get("Notification_OrderStatusChanged", $"Your order {order.OrderNumber} status changed to {request.NewStatus}", order.OrderNumber, request.NewStatus);
                _context.Notifications.Add(Notification.Create(
                    order.CustomerId.Value,
                    NotificationType.OrderStatusChanged,
                    notifMsg,
                    order.Id
                ));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new BulkUpdateOrderStatusResponse(orders.Count, foundIds));
    }
}
