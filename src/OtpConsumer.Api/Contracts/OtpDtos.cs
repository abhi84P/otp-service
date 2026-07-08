namespace OtpConsumer.Api.Contracts;

public record RequestOtpRequest(string Identifier);

public record RequestOtpResponse(string RequestId, DateTime ExpiresAt, string? Code);

public record VerifyOtpRequest(string RequestId, string Code);

public record VerifyOtpResponse(bool Verified, string? Reason);
