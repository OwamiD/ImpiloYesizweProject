
using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using Microsoft.AspNetCore.Http;

namespace ImpiloYesizweProject.Services
{
    public class AuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Record(
            string action,
            string entityType,
            int? entityId,
            string description,
            string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action is required.");

            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("Entity type is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");

            var httpContext = _httpContextAccessor.HttpContext;

            // Prefer the authenticated cookie identity.
            string? username = null;

            if (httpContext?.User.Identity?.IsAuthenticated == true)
            {
                username = httpContext.User.Identity.Name;
            }

            // Temporary backwards compatibility with session login.
            if (string.IsNullOrWhiteSpace(username))
            {
                username = httpContext?.Session.GetString("Admin");
            }

            var auditLog = new AuditLog
            {
                AdminUsername = string.IsNullOrWhiteSpace(username)
                    ? "Unknown"
                    : username.Trim(),

                Action = action.Trim(),
                EntityType = entityType.Trim(),
                EntityId = entityId,
                Description = description.Trim(),
                Reason = reason?.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
        }
    }
}
