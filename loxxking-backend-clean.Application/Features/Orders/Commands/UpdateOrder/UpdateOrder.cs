using loxxking_backend_clean.Domain.Entities.Orders;

namespace loxxking_backend_clean.Application.Features.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid Id, string CustomerName, string Phone, string Address, string ShipmentCode, Guid EditorId) : IRequest<Result>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateOrderHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order == null) return Result.Failure(new Error("Error.NotFound", "Order_NotFound"));

        _context.OrderEditLogs.Add(new OrderEditLog { Id = Guid.NewGuid(), OrderId = order.Id, EditedBy = request.EditorId, EditedAt = DateTime.UtcNow, FieldName = "All", NewValue = "Full update" });

        order.UpdateDetails(request.Phone, request.Address, request.ShipmentCode);
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
