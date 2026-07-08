namespace OtpManagement.Service.Domain;

public enum OtpStatus
{
    Active = 0,
    Verified = 1,
    Locked = 2
}

public class OtpRequest
{
    public Guid RequestId { get; set; }
    public required string Identifier { get; set; }
    public required string CodeHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int AttemptCount { get; set; }
    public OtpStatus Status { get; set; }
}
