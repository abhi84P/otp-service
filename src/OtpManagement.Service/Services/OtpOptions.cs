namespace OtpManagement.Service.Services;

public class OtpOptions
{
    public const string SectionName = "Otp";

    public int CodeLength { get; set; } = 6;
    public int ExpirySeconds { get; set; } = 120;
    public int MaxAttemptsPerOtp { get; set; } = 5;
    public int IdentifierFailureThreshold { get; set; } = 10;
    public int IdentifierFreezeSeconds { get; set; } = 900;
    public string HmacSecret { get; set; } = string.Empty;
    public bool ReturnCodeInResponse { get; set; }
}
