namespace BulkApi.Application.DTOs;

public sealed record ReceiptResponse(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAtUtc
);
