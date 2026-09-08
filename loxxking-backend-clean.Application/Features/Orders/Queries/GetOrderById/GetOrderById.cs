using loxxking_backend_clean.Domain.Entities.Orders;

namespace loxxking_backend_clean.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderResponse>>;

public record OrderResponse(Guid Id, string CustomerName, string Phone, string PaymentMethod, string PaymentStatus, string OrderStatus, decimal TotalAmount, string ShipmentCode);

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetOrderByIdHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.Where(o => o.Id == request.Id)
            .Select(o => new OrderResponse(o.Id, o.Customer != null ? o.Customer.Name : "Guest", o.Phone ?? "", o.PaymentMethod.ToString(), o.PaymentStatus.ToString(), o.Status.ToString(), o.TotalAmount.Value, o.ShipmentCode ?? ""))
            .FirstOrDefaultAsync(cancellationToken);
            
        if (order == null) return Result.Failure<OrderResponse>(new Error("Error.NotFound", "Order_NotFound"));
        return Result.Success(order);
    }
}
