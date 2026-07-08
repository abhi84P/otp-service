namespace OtpManagement.Service.Domain;

public enum OtpValidationOutcome
{
    Valid,
    NotFound,
    InvalidCode,
    Expired,
    AlreadyUsed,
    TooManyAttempts,
    IdentifierFrozen
}
