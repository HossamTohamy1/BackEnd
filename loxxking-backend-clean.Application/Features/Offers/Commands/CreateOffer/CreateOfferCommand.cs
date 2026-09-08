namespace loxxking_backend_clean.Application.Features.Offers.Commands.CreateOffer;

public record CreateOfferCommand(
    Guid ProductId,
    decimal DiscountPercent,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<CreateOfferResponse>>;

public record CreateOfferResponse(Guid Id);
