using BulkApi.Application.DTOs;
using BulkApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        if (string.IsNullOrWhiteSpace(cmd.IdempotencyKey))
            return await CreateReceiptAsync(cmd.Request, ct);

        var existing = await TryGetExistingResponseAsync(cmd.IdempotencyKey!, ct);
        if (existing is not null)
            return existing;

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var (idempotencyRecord, cachedResponse) =
            await TryReserveIdempotencyKeyAsync(cmd.IdempotencyKey!, transaction, ct);

        // 日本語コメント: 既存レスポンスがあればそのまま返して重複処理を避ける
        // English comment: Return early when an existing response is found to prevent duplicate handling
        if (cachedResponse is not null)
            return cachedResponse;

        if (idempotencyRecord is null)
        {
            // 日本語コメント: 他のトランザクションがレスポンスを書き込むのを短時間待つ
            // English comment: Briefly wait for another transaction to finish persisting the response
            var awaitedResponse = await WaitForExistingResponseAsync(cmd.IdempotencyKey!, ct);

            if (awaitedResponse is not null)
                return awaitedResponse;

            throw new InvalidOperationException("Idempotency reservation failed without a cached response.");
        }

        try
        {
            var response = await CreateReceiptAsync(cmd.Request, ct);

            idempotencyRecord.SetResponse(JsonSerializer.Serialize(response));
            await _db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<(IdempotencyRecord? record, ReceiptResponse? response)> TryReserveIdempotencyKeyAsync(
        string idempotencyKey,
        IDbContextTransaction transaction,
        CancellationToken ct)
    {
        try
        {
            var reservedRecord = new IdempotencyRecord(idempotencyKey, string.Empty);
            _db.IdempotencyRecords.Add(reservedRecord);
            await _db.SaveChangesAsync(ct);

            return (reservedRecord, null);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);

            var saved = await TryGetExistingResponseAsync(idempotencyKey, ct);
            return (null, saved);
        }
    }

    private async Task<ReceiptResponse?> TryGetExistingResponseAsync(string idempotencyKey, CancellationToken ct)
    {
        var hit = await _db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == idempotencyKey, ct);

        if (hit is null || string.IsNullOrWhiteSpace(hit.ResponseJson))
            return null;

        return JsonSerializer.Deserialize<ReceiptResponse>(hit.ResponseJson)!;
    }

    private async Task<ReceiptResponse?> WaitForExistingResponseAsync(string idempotencyKey, CancellationToken ct)
    {
        const int maxAttempts = 5;
        const int delayMs = 50;

        // 日本語コメント: 既存トランザクションがレスポンスを保存するのを何度か再確認する
        // English comment: Retry a few times to see if another transaction persisted the response
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await TryGetExistingResponseAsync(idempotencyKey, ct);
            if (response is not null)
                return response;

            await Task.Delay(delayMs, ct);
        }

        return null;
    }

    private async Task<ReceiptResponse> CreateReceiptAsync(CreateReceiptRequest request, CancellationToken ct)
    {
        var receipt = new Receipt(request.Title, request.Amount, request.Currency);
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        return new ReceiptResponse(
            receipt.Id, receipt.Title, receipt.Amount, receipt.Currency, receipt.Status, receipt.CreatedAtUtc);
    }
}
