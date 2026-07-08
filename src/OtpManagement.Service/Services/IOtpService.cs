using OtpManagement.Service.Domain;

namespace OtpManagement.Service.Services;

public interface IOtpService
{
    Task<OtpGeneration> GenerateAsync(string identifier, CancellationToken cancellationToken = default);
    Task<OtpValidationOutcome> ValidateAsync(string requestId, string code, CancellationToken cancellationToken = default);
}
