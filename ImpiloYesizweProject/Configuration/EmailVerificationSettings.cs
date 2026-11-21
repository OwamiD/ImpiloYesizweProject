namespace ImpiloYesizweProject.Configuration
{
    public class EmailVerificationSettings
    {
        public string HashKey { get; set; } = string.Empty;

        public int CodeLifetimeMinutes { get; set; } = 5;

        public int MaxFailedAttempts { get; set; } = 5;

        public int ResendCooldownSeconds { get; set; } = 60;

        public int MaxResends { get; set; } = 3;
    }
}