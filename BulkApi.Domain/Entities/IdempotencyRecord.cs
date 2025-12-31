namespace BulkApi.Domain.Entities;

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
}
