using Grpc.Core;
using OtpConsumer.Api.Contracts;
using OtpService.Grpc;

namespace OtpConsumer.Api.Endpoints;

public static class OtpEndpoints
{
    public static void MapOtpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/otp");

        group.MapPost("/request", RequestOtp);
        group.MapPost("/verify", VerifyOtp);
    }

    private static async Task<IResult> RequestOtp(
        RequestOtpRequest request,
        OtpManager.OtpManagerClient client,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
            return Results.BadRequest(new { error = "identifier is required" });

        try
        {
            var response = await client.GenerateOtpAsync(
                new GenerateOtpRequest { Identifier = request.Identifier },
                cancellationToken: cancellationToken);

            return Results.Ok(new RequestOtpResponse(
                response.RequestId,
                response.ExpiresAt.ToDateTime(),
                string.IsNullOrEmpty(response.Code) ? null : response.Code));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.ResourceExhausted)
        {
            var retryAfter = ex.Trailers.GetValue("retry-after-seconds");
            return Results.Json(
                new { error = "OTP request rejected, try again later", retryAfterSeconds = retryAfter },
                statusCode: StatusCodes.Status429TooManyRequests);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            return Results.BadRequest(new { error = ex.Status.Detail });
        }
    }

    private static async Task<IResult> VerifyOtp(
        VerifyOtpRequest request,
        OtpManager.OtpManagerClient client,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId) || string.IsNullOrWhiteSpace(request.Code))
            return Results.BadRequest(new { error = "requestId and code are required" });

        var response = await client.ValidateOtpAsync(
            new ValidateOtpRequest { RequestId = request.RequestId, Code = request.Code },
            cancellationToken: cancellationToken);

        return response.Status switch
        {
            ValidationStatus.Valid =>
                Results.Ok(new VerifyOtpResponse(true, null)),
            ValidationStatus.IdentifierFrozen =>
                Results.Json(
                    new VerifyOtpResponse(false, Reason(response.Status)),
                    statusCode: StatusCodes.Status429TooManyRequests),
            _ =>
                Results.BadRequest(new VerifyOtpResponse(false, Reason(response.Status)))
        };
    }

    private static string Reason(ValidationStatus status) => status switch
    {
        ValidationStatus.NotFound => "request_not_found",
        ValidationStatus.InvalidCode => "invalid_code",
        ValidationStatus.Expired => "expired",
        ValidationStatus.AlreadyUsed => "already_used",
        ValidationStatus.TooManyAttempts => "too_many_attempts",
        ValidationStatus.IdentifierFrozen => "identifier_frozen",
        _ => "unknown"
    };
}
