namespace loxxking_backend_clean.Application.Features.Offers.Queries.GetOfferById;

public record GetOfferByIdQuery(Guid Id) : IRequest<Result<GetOfferByIdResponse>>;

public record GetOfferByIdResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal DiscountPercent,
    DateTime StartDate,
    DateTime EndDate
);
