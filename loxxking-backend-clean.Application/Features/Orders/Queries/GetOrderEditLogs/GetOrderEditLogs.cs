namespace loxxking_backend_clean.Application.Features.Orders.Queries.GetOrderEditLogs;

public record GetOrderEditLogsQuery(Guid OrderId) : IRequest<Result<List<EditLogResponse>>>;
public record EditLogResponse(Guid Id, string EditorName, string Changes, DateTime EditedAt);

public class GetOrderEditLogsHandler : IRequestHandler<GetOrderEditLogsQuery, Result<List<EditLogResponse>>>
{
    private readonly IApplicationDbContext _context;
    public GetOrderEditLogsHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<List<EditLogResponse>>> Handle(GetOrderEditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _context.OrderEditLogs.Where(l => l.OrderId == request.OrderId)
            .Select(l => new EditLogResponse(l.Id, l.Editor != null ? l.Editor.Name : "System", l.FieldName + " " + l.NewValue ?? "", l.EditedAt))
            .ToListAsync(cancellationToken);
        return Result.Success(logs);
    }
}
