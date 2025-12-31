using BulkApi.Application.DTOs;
using BulkApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using BulkApi.Infrastructure;

namespace BulkApi.Application.Receipts;

public sealed record CreateReceiptCommand(CreateReceiptRequest Request, string? IdempotencyKey)
    : IRequest<ReceiptResponse>;

public sealed class CreateReceiptCommandHandler
    : IRequestHandler<CreateReceiptCommand, ReceiptResponse>
{
    private readonly AppDbContext _db;
    public CreateReceiptCommandHandler(AppDbContext db) => _db = db;

    public async Task<ReceiptResponse> Handle(CreateReceiptCommand cmd, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(cmd.IdempotencyKey))
        {
            var hit = await _db.IdempotencyRecords.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == cmd.IdempotencyKey, ct);

            if (hit is not null)
                return JsonSerializer.Deserialize<ReceiptResponse>(hit.ResponseJson)!;
        }

        var receipt = new Receipt(cmd.Request.Title, cmd.Request.Amount, cmd.Request.Currency);
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        var response = new ReceiptResponse(
            receipt.Id, receipt.Title, receipt.Amount, receipt.Currency, receipt.Status, receipt.CreatedAtUtc);

        if (!string.IsNullOrWhiteSpace(cmd.IdempotencyKey))
        {
            try
            {
                _db.IdempotencyRecords.Add(new IdempotencyRecord(
                    cmd.IdempotencyKey, JsonSerializer.Serialize(response)));
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                var saved = await _db.IdempotencyRecords.AsNoTracking()
                    .FirstAsync(x => x.Key == cmd.IdempotencyKey, ct);

                return JsonSerializer.Deserialize<ReceiptResponse>(saved.ResponseJson)!;
            }
        }

        return response;
    }
}
