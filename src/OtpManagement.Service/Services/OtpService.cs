using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpManagement.Service.Data;
using OtpManagement.Service.Domain;

namespace OtpManagement.Service.Services;

public class OtpService(OtpDbContext db, IOptions<OtpOptions> options, TimeProvider timeProvider) : IOtpService
{
    private readonly OtpOptions _options = options.Value;

    public async Task<OtpGeneration> GenerateAsync(string identifier, CancellationToken cancellationToken = default)
    {
        identifier = Normalize(identifier);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var state = await db.IdentifierStates.FindAsync([identifier], cancellationToken);
        if (state?.FrozenUntil is { } frozenUntil && frozenUntil > now)
            return OtpGeneration.Rejected(frozenUntil - now);

        var activeOtp = await db.OtpRequests
            .Where(o => o.Identifier == identifier && o.Status == OtpStatus.Active && o.ExpiresAt > now)
            .OrderByDescending(o => o.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (activeOtp is not null)
            return OtpGeneration.Rejected(activeOtp.ExpiresAt - now);

        var code = GenerateCode();
        var otp = new OtpRequest
        {
            RequestId = Guid.NewGuid(),
            Identifier = identifier,
            CodeHash = ComputeHash(code),
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(_options.ExpirySeconds),
            Status = OtpStatus.Active
        };

        db.OtpRequests.Add(otp);
        await db.SaveChangesAsync(cancellationToken);

        return OtpGeneration.Granted(otp.RequestId, otp.ExpiresAt, code);
    }

    public async Task<OtpValidationOutcome> ValidateAsync(string requestId, string code, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(requestId, out var id))
            return OtpValidationOutcome.NotFound;

        var otp = await db.OtpRequests.FindAsync([id], cancellationToken);
        if (otp is null)
            return OtpValidationOutcome.NotFound;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var state = await db.IdentifierStates.FindAsync([otp.Identifier], cancellationToken);
        if (state?.FrozenUntil is { } frozenUntil && frozenUntil > now)
            return OtpValidationOutcome.IdentifierFrozen;

        if (otp.Status == OtpStatus.Verified)
            return OtpValidationOutcome.AlreadyUsed;

        if (otp.Status == OtpStatus.Locked)
            return OtpValidationOutcome.TooManyAttempts;

        if (otp.ExpiresAt <= now)
            return OtpValidationOutcome.Expired;

        var submittedHash = ComputeHash(code);
        var matches = CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(otp.CodeHash),
            Convert.FromHexString(submittedHash));

        if (matches)
        {
            otp.Status = OtpStatus.Verified;
            otp.VerifiedAt = now;
            if (state is not null)
            {
                state.FailedCount = 0;
                state.FrozenUntil = null;
            }
            await db.SaveChangesAsync(cancellationToken);
            return OtpValidationOutcome.Valid;
        }

        otp.AttemptCount++;
        if (otp.AttemptCount >= _options.MaxAttemptsPerOtp)
            otp.Status = OtpStatus.Locked;

        if (state is null)
        {
            state = new IdentifierState { Identifier = otp.Identifier };
            db.IdentifierStates.Add(state);
        }
        state.FailedCount++;
        if (state.FailedCount >= _options.IdentifierFailureThreshold)
        {
            state.FrozenUntil = now.AddSeconds(_options.IdentifierFreezeSeconds);
            state.FailedCount = 0;
        }

        await db.SaveChangesAsync(cancellationToken);
        return otp.Status == OtpStatus.Locked
            ? OtpValidationOutcome.TooManyAttempts
            : OtpValidationOutcome.InvalidCode;
    }

    private static string Normalize(string identifier) => identifier.Trim().ToLowerInvariant();

    private string GenerateCode()
    {
        var max = (int)Math.Pow(10, _options.CodeLength);
        return RandomNumberGenerator.GetInt32(0, max).ToString($"D{_options.CodeLength}");
    }

    private string ComputeHash(string code)
    {
        var key = Encoding.UTF8.GetBytes(_options.HmacSecret);
        var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash);
    }
}
