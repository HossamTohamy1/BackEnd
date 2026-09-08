namespace loxxking_backend_clean.Application.Features.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(Guid Id) : IRequest<Result>;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteOrderHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order == null) return Result.Failure(new Error("Error.NotFound", "Order_NotFound"));
        

        order.IsDeleted = true;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
