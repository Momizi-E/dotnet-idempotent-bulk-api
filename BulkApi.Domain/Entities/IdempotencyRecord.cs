namespace BulkApi.Domain.Entities;

using BulkApi.Domain.Exceptions;

public sealed class IdempotencyRecord
{
    public long Id { get; private set; }
    public string Key { get; private set; } = default!;
    public string ResponseJson { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private IdempotencyRecord() { }

    public IdempotencyRecord(string key, string responseJson)
    {
        Key = key;
        ResponseJson = responseJson;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetResponse(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            throw new DomainException("ResponseJson cannot be empty.");

        if (!string.IsNullOrWhiteSpace(ResponseJson))
            throw new DomainException("ResponseJson has already been set for this idempotency record.");

        ResponseJson = responseJson;
    }
}
