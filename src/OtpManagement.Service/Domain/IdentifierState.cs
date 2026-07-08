namespace OtpManagement.Service.Domain;

public class IdentifierState
{
    public required string Identifier { get; set; }
    public int FailedCount { get; set; }
    public DateTime? FrozenUntil { get; set; }
}
