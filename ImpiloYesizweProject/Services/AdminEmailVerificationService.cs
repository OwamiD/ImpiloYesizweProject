using ImpiloYesizweProject.Configuration;
using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace ImpiloYesizweProject.Services
{
    public class AdminEmailVerificationService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly EmailVerificationSettings _settings;
        private readonly byte[] _hashKey;
        private readonly ILogger<AdminEmailVerificationService> _logger;

        public AdminEmailVerificationService(
            AppDbContext context,
            EmailService emailService,
            IOptions<EmailVerificationSettings> options,
            ILogger<AdminEmailVerificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _settings = options.Value;
            _logger = logger;

            _hashKey = Convert.FromBase64String(_settings.HashKey);

            if (_hashKey.Length != 32)
            {
                throw new InvalidOperationException(
                    "The email verification HashKey must contain 32 bytes.");
            }
        }

        // Create a new challenge and send the initial verification code.
        public async Task<Guid> CreateAndSendAsync(
            AdminUser admin,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(admin);

            if (admin.Id <= 0 ||
                string.IsNullOrWhiteSpace(admin.Email))
            {
                throw new InvalidOperationException(
                    "A valid administrator account and email are required.");
            }

            var now = DateTime.UtcNow;
            var challengeId = Guid.NewGuid();
            var code = GenerateCode();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Invalidate previous outstanding challenges for this administrator.
            var previousChallenges =
                await _context.AdminEmailVerifications
                    .Where(v =>
                        v.AdminUserId == admin.Id &&
                        v.UsedAtUtc == null)
                    .ToListAsync(cancellationToken);

            foreach (var previous in previousChallenges)
            {
                previous.UsedAtUtc = now;
            }

            var challenge = new AdminEmailVerification
            {
                ChallengeId = challengeId,
                AdminUserId = admin.Id,
                CodeHash = HashCode(challengeId, admin.Id, code),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(
                    _settings.CodeLifetimeMinutes),
                LastSentAtUtc = now,
                FailedAttempts = 0,
                ResendCount = 0
            };

            _context.AdminEmailVerifications.Add(challenge);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            try
            {
                await SendCodeAsync(admin, code, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send administrator verification email. Admin ID: {AdminUserId}",
                    admin.Id);

                // Invalidate the challenge if the initial email cannot be sent.
                challenge.UsedAtUtc = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(
                        cleanupException,
                        "Could not invalidate an unsent verification challenge.");
                }

                throw;
            }

            return challengeId;
        }

        // Verify a submitted code and consume it after successful verification.
        public async Task<AdminUser?> VerifyAsync(
            Guid challengeId,
            string code,
            CancellationToken cancellationToken = default)
        {
            if (challengeId == Guid.Empty)
                return null;

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var challenge =
                await _context.AdminEmailVerifications
                    .Include(v => v.AdminUser)
                    .SingleOrDefaultAsync(
                        v => v.ChallengeId == challengeId,
                        cancellationToken);

            if (challenge == null ||
                challenge.UsedAtUtc != null ||
                challenge.ExpiresAtUtc <= DateTime.UtcNow ||
                challenge.FailedAttempts >= _settings.MaxFailedAttempts)
            {
                return null;
            }

            bool validCodeFormat =
                !string.IsNullOrEmpty(code) &&
                code.Length == 6 &&
                code.All(c => c >= '0' && c <= '9');

            bool validCode =
                validCodeFormat &&
                CodeMatches(challenge, code);

            if (!validCode)
            {
                challenge.FailedAttempts++;

                if (challenge.FailedAttempts >=
                    _settings.MaxFailedAttempts)
                {
                    challenge.UsedAtUtc = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return null;
            }

            // Mark the challenge as used before granting access.
            challenge.UsedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return challenge.AdminUser;
        }

        // Resend a code while enforcing cooldown and resend limits.
        public async Task<bool> ResendAsync(
            Guid challengeId,
            CancellationToken cancellationToken = default)
        {
            if (challengeId == Guid.Empty)
                return false;

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var challenge =
                await _context.AdminEmailVerifications
                    .Include(v => v.AdminUser)
                    .SingleOrDefaultAsync(
                        v => v.ChallengeId == challengeId,
                        cancellationToken);

            if (challenge == null ||
                challenge.UsedAtUtc != null ||
                challenge.ExpiresAtUtc <= DateTime.UtcNow ||
                challenge.FailedAttempts >= _settings.MaxFailedAttempts ||
                challenge.ResendCount >= _settings.MaxResends)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            if (challenge.LastSentAtUtc.HasValue &&
                now < challenge.LastSentAtUtc.Value.AddSeconds(
                    _settings.ResendCooldownSeconds))
            {
                return false;
            }

            var newCode = GenerateCode();

            challenge.CodeHash = HashCode(
                challenge.ChallengeId,
                challenge.AdminUserId,
                newCode);

            challenge.ExpiresAtUtc =
                now.AddMinutes(_settings.CodeLifetimeMinutes);

            challenge.LastSentAtUtc = now;
            challenge.ResendCount++;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            try
            {
                await SendCodeAsync(
                    challenge.AdminUser,
                    newCode,
                    cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to resend administrator verification email. Admin ID: {AdminUserId}",
                    challenge.AdminUserId);

                // Invalidate the challenge because the replacement
                // code may not have reached the administrator.
                challenge.UsedAtUtc = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(
                        cleanupException,
                        "Could not invalidate the failed resend challenge.");
                }

                return false;
            }
        }

        // Generate a cryptographically secure six-digit code.
        private static string GenerateCode()
        {
            return RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6", CultureInfo.InvariantCulture);
        }

        // Hash the code using HMAC-SHA256.
        // Bind the hash to its challenge and administrator.
        private string HashCode(
            Guid challengeId,
            int adminUserId,
            string code)
        {
            byte[] payload = Encoding.UTF8.GetBytes(
                $"{challengeId:N}:{adminUserId}:{code}");

            try
            {
                using var hmac = new HMACSHA256(_hashKey);

                return Convert.ToHexString(
                    hmac.ComputeHash(payload));
            }
            finally
            {
                CryptographicOperations.ZeroMemory(payload);
            }
        }

        // Compare hashes without exposing timing differences.
        private bool CodeMatches(
            AdminEmailVerification challenge,
            string submittedCode)
        {
            if (string.IsNullOrWhiteSpace(challenge.CodeHash))
                return false;

            try
            {
                byte[] storedHash =
                    Convert.FromHexString(challenge.CodeHash);

                byte[] calculatedHash =
                    Convert.FromHexString(
                        HashCode(
                            challenge.ChallengeId,
                            challenge.AdminUserId,
                            submittedCode));

                return storedHash.Length == calculatedHash.Length &&
                    CryptographicOperations.FixedTimeEquals(
                        storedHash,
                        calculatedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Send the verification email.
        private Task SendCodeAsync(
            AdminUser admin,
            string code,
            CancellationToken cancellationToken)
        {
            string safeName = HtmlEncoder.Default.Encode(
                string.IsNullOrWhiteSpace(admin.FullName)
                    ? "Administrator"
                    : admin.FullName);

            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif;color:#333;">
                    <h2>ImpiloYesizwe Care Community Project</h2>

                    <p>Hello {safeName},</p>

                    <p>
                        We received a sign-in request for your
                        administrator account.
                    </p>

                    <p>Your verification code is:</p>

                    <h1 style="letter-spacing:8px;color:#198754;">
                        {code}
                    </h1>

                    <p>
                        This code expires in
                        {_settings.CodeLifetimeMinutes} minutes.
                    </p>

                    <p>
                        Do not share this code with anyone.
                    </p>

                    <p>
                        If you did not attempt to sign in,
                        you can ignore this email.
                    </p>

                    <hr />

                    <small>
                        ImpiloYesizwe Care Community Project
                    </small>
                </body>
                </html>
                """;

            string textBody = $"""
                Hello {admin.FullName},

                Your ImpiloYesizwe administrator verification code is: {code}

                The code expires in {_settings.CodeLifetimeMinutes} minutes.

                Do not share this code with anyone.
                """;

            return _emailService.SendEmailAsync(
                recipientEmail: admin.Email,
                subject: "Your ImpiloYesizwe sign-in verification code",
                htmlBody: htmlBody,
                textBody: textBody,
                cancellationToken: cancellationToken);
        }
    }
}
