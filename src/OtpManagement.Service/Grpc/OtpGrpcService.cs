using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using OtpManagement.Service.Domain;
using OtpManagement.Service.Services;
using OtpService.Grpc;

namespace OtpManagement.Service.Grpc;

public class OtpGrpcService(IOtpService otpService, IOptions<OtpOptions> options) : OtpManager.OtpManagerBase
{
    private readonly OtpOptions _options = options.Value;

    public override async Task<GenerateOtpResponse> GenerateOtp(GenerateOtpRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "identifier is required"));

        var result = await otpService.GenerateAsync(request.Identifier, context.CancellationToken);

        if (!result.Succeeded)
        {
            var retryAfterSeconds = (int)Math.Ceiling(result.RetryAfter!.Value.TotalSeconds);
            var trailers = new Metadata { { "retry-after-seconds", retryAfterSeconds.ToString() } };
            throw new RpcException(
                new Status(StatusCode.ResourceExhausted, "an active OTP already exists or the identifier is frozen"),
                trailers);
        }

        var response = new GenerateOtpResponse
        {
            RequestId = result.RequestId.ToString(),
            ExpiresAt = Timestamp.FromDateTime(DateTime.SpecifyKind(result.ExpiresAt, DateTimeKind.Utc))
        };

        if (_options.ReturnCodeInResponse)
            response.Code = result.Code;

        return response;
    }

    public override async Task<ValidateOtpResponse> ValidateOtp(ValidateOtpRequest request, ServerCallContext context)
    {
        var outcome = await otpService.ValidateAsync(request.RequestId, request.Code, context.CancellationToken);

        return new ValidateOtpResponse { Status = ToProto(outcome) };
    }

    private static ValidationStatus ToProto(OtpValidationOutcome outcome) => outcome switch
    {
        OtpValidationOutcome.Valid => ValidationStatus.Valid,
        OtpValidationOutcome.NotFound => ValidationStatus.NotFound,
        OtpValidationOutcome.InvalidCode => ValidationStatus.InvalidCode,
        OtpValidationOutcome.Expired => ValidationStatus.Expired,
        OtpValidationOutcome.AlreadyUsed => ValidationStatus.AlreadyUsed,
        OtpValidationOutcome.TooManyAttempts => ValidationStatus.TooManyAttempts,
        OtpValidationOutcome.IdentifierFrozen => ValidationStatus.IdentifierFrozen,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
    };
}
