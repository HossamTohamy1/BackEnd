namespace loxxking_backend_clean.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    OrderStatus? Status,
    Guid? CountryId,
    PaymentMethod? PaymentMethod,
    string? Search,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<GetOrdersResponse>>;

public record GetOrdersResponse(
    object Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
