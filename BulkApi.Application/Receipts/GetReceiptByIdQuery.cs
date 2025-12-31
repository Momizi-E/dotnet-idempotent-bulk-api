using BulkApi.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BulkApi.Infrastructure;

namespace BulkApi.Application.Receipts;

public sealed record GetReceiptByIdQuery(Guid Id) : IRequest<ReceiptResponse?>;

public sealed class GetReceiptByIdQueryHandler
    : IRequestHandler<GetReceiptByIdQuery, ReceiptResponse?>
{
    private readonly AppDbContext _db;
    public GetReceiptByIdQueryHandler(AppDbContext db) => _db = db;

    public async Task<ReceiptResponse?> Handle(GetReceiptByIdQuery q, CancellationToken ct)
    {
        var r = await _db.Receipts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == q.Id, ct);
        return r is null ? null : new ReceiptResponse(r.Id, r.Title, r.Amount, r.Currency, r.Status, r.CreatedAtUtc);
    }
}
