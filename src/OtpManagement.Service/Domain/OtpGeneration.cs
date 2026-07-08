namespace OtpManagement.Service.Domain;

public sealed record OtpGeneration
{
    public required bool Succeeded { get; init; }
    public Guid RequestId { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? Code { get; init; }
    public TimeSpan? RetryAfter { get; init; }

    public static OtpGeneration Granted(Guid requestId, DateTime expiresAt, string code) =>
        new() { Succeeded = true, RequestId = requestId, ExpiresAt = expiresAt, Code = code };

    public static OtpGeneration Rejected(TimeSpan retryAfter) =>
        new() { Succeeded = false, RetryAfter = retryAfter };
}
