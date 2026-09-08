namespace loxxking_backend_clean.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status, Guid UserId) : IRequest<Result>;
