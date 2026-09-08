namespace loxxking_backend_clean.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<GetOrdersResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var query = _context.Orders.AsQueryable();

        if (request.Status.HasValue) query = query.Where(o => o.Status == request.Status.Value);
        if (request.CountryId.HasValue) query = query.Where(o => o.CountryId == request.CountryId.Value);
        if (request.PaymentMethod.HasValue) query = query.Where(o => o.PaymentMethod == request.PaymentMethod.Value);
        if (request.DateFrom.HasValue) query = query.Where(o => o.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue) query = query.Where(o => o.CreatedAt <= request.DateTo.Value);
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(o =>
                o.Phone.ToLower().Contains(s) ||
                o.Address.ToLower().Contains(s) ||
                (o.ShipmentCode != null && o.ShipmentCode.ToLower().Contains(s)) ||
                (o.Customer != null && o.Customer.Name.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                CustomerName = o.Customer != null ? o.Customer.Name : (o.GuestName ?? "Guest"),
                o.Phone,
                o.Address,
                Country = o.Country.Name,
                Status = o.Status.ToString(),
                PaymentMethod = o.PaymentMethod.ToString(),
                PaymentStatus = o.PaymentStatus.ToString(),
                Total = o.TotalAmount.Value,
                TotalAmount = o.TotalAmount.Value,
                o.City,
                o.Area,
                o.DeliveryCompany,
                o.Gender,
                o.EstimatedDelivery,
                o.ShipmentCode,
                o.Notes,
                o.BankTransferReceiptUrl,
                o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var transfers = await _context.BankTransfers
            .Where(bt => orderIds.Contains(bt.OrderId))
            .Select(bt => new
            {
                bt.Id,
                bt.OrderId,
                bt.ProofImageUrl,
                Status = bt.Status.ToString(),
                bt.SubmittedAt
            })
            .ToListAsync(cancellationToken);

        var transfersLookup = transfers.ToLookup(t => t.OrderId);

        var data = orders.Select(o => new
        {
            o.Id,
            o.OrderNumber,
            o.CustomerName,
            o.Phone,
            o.Address,
            o.Country,
            o.Status,
            o.PaymentMethod,
            o.PaymentStatus,
            o.Total,
            o.TotalAmount,
            o.City,
            o.Area,
            o.DeliveryCompany,
            o.Gender,
            o.EstimatedDelivery,
            o.ShipmentCode,
            o.Notes,
            o.CreatedAt,
            BankTransfers = transfersLookup[o.Id].Select(t => new
            {
                t.Id,
                t.ProofImageUrl,
                t.Status,
                t.SubmittedAt
            }).ToList(),
            ProofImageUrl = transfersLookup[o.Id].Select(t => t.ProofImageUrl).FirstOrDefault() ?? o.BankTransferReceiptUrl
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result.Success(new GetOrdersResponse(data, totalCount, page, pageSize, totalPages));
    }
}
