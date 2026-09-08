namespace loxxking_backend_clean.Application.Features.Orders.Queries.TrackOrder;

public record TrackOrderQuery(string Phone, string ShipmentCode) : IRequest<Result<string>>;

public class TrackOrderHandler : IRequestHandler<TrackOrderQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public TrackOrderHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<string>> Handle(TrackOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.Where(o => o.Phone == request.Phone && o.ShipmentCode == request.ShipmentCode)
            .Select(o => o.Status.ToString())
            .FirstOrDefaultAsync(cancellationToken);
            
        if (order == null) return Result.Failure<string>(new Error("Error.NotFound", "Order_NotFound"));
        return Result.Success(order);
    }
}
