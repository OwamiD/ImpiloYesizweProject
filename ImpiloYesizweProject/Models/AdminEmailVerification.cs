using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class AdminEmailVerification
    {
        public int Id { get; set; }

        // Identifies a specific login verification challenge.
        public Guid ChallengeId { get; set; }

        // Links verification to an administrator.
        public int AdminUserId { get; set; }

        public AdminUser AdminUser { get; set; } = null!;

        // Store a hash, never the actual six-digit code.
        [Required]
        [StringLength(100)]
        public string CodeHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? UsedAtUtc { get; set; }

        public DateTime? LastSentAtUtc { get; set; }

        public int FailedAttempts { get; set; }

        public int ResendCount { get; set; }
    }
}