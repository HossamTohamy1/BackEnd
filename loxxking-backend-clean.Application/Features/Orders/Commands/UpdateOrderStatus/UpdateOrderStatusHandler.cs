using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.Orders;


namespace loxxking_backend_clean.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public UpdateOrderStatusHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null) return Result.Failure(new Error("Error.NotFound", "Order_NotFound"));

        var oldStatus = order.Status.ToString();
        order.ChangeStatus(request.Status);

        _context.OrderEditLogs.Add(new OrderEditLog
        {
            OrderId = order.Id,
            EditedBy = request.UserId,
            FieldName = "Status",
            OldValue = oldStatus,
            NewValue = request.Status.ToString(),
            EditedAt = DateTime.UtcNow
        });

        _context.Orders.Update(order);

        if (order.CustomerId.HasValue)
        {
            var notifMsg = _localizer.Get("Notification_OrderStatusChanged", $"Your order {order.OrderNumber} status changed to {request.Status}", order.OrderNumber, request.Status);
            _context.Notifications.Add(Notification.Create(
                order.CustomerId.Value,
                NotificationType.OrderStatusChanged,
                notifMsg,
                order.Id
            ));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
