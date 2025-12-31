namespace BulkApi.Application.DTOs;

public sealed record CreateReceiptRequest(string Title, decimal Amount, string Currency);
