using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using ImpiloYesizweProject.Services;
using ImpiloYesizweProject.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;



namespace ImpiloYesizweProject.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;
        private readonly ILogger<AdminController> _logger;
        private readonly AdminEmailVerificationService _verificationService;

        public AdminController
            (
            AppDbContext context,
            AuditService auditService,
            IWebHostEnvironment environment,
            EmailService emailService,
            AdminEmailVerificationService verificationService,
            ILogger<AdminController> logger
            )
        {
            _context = context;
            _auditService = auditService;
            _environment = environment;
            _emailService = emailService;
            _verificationService = verificationService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            // Only include active donations.
            var activeDonations = _context.Donations
                .Where(d => !d.IsVoided);

            var model = new DashboardViewModel
            {
                TotalMessages = _context.ContactMessages.Count(),

                TotalDonations = activeDonations.Count(),

                TotalGalleryImages = _context.GalleryImages.Count(),

                TotalDonationAmount =
                    activeDonations.Sum(d => (decimal?)d.Amount) ?? 0m,

                RecentMessages = _context.ContactMessages
                    .OrderByDescending(x => x.DateSent)
                    .Take(5)
                    .ToList(),

                RecentDonations = activeDonations
                    .OrderByDescending(x => x.DonationDate)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        // GET: Admin/TestEmail
        [HttpGet]
        public IActionResult TestEmail()
        {
            if (!IsLoggedIn())
                return RedirectToAction(nameof(Login));

            if (!_environment.IsDevelopment())
                return NotFound();

            return View();
        }

        // POST: Admin/TestEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(
            CancellationToken cancellationToken)
        {
            if (!IsLoggedIn())
                return RedirectToAction(nameof(Login));

            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                await _emailService.SendEmailAsync(
                    recipientEmail: "owamidmsane@gmail.com",
                    subject: "ImpiloYesizwe Email Test",
                    htmlBody: """
                <h2>Email service is working!</h2>
                <p>Your ImpiloYesizwe Gmail integration
                successfully sent this test message.</p>
                <p>You can now proceed with administrator
                email verification.</p>
                """,
                    textBody:
                        "Email service is working! Your ImpiloYesizwe Gmail integration successfully sent this test message.",
                    cancellationToken: cancellationToken
                );

                TempData["Success"] = "Test email sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImpiloYesizwe test email failed.");

                TempData["Error"] =
                    "Email could not be sent. Check Gmail configuration and application logs.";
            }

            return RedirectToAction(nameof(TestEmail));
        }

        public IActionResult Messages()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            var messages = _context.ContactMessages
                                   .OrderByDescending(x => x.DateSent)
                                   .ToList();

            return View(messages);
        }
        public IActionResult Donations()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            var donations = _context.Donations
                                    .OrderByDescending(x => x.DonationDate)
                                    .ToList();

            return View(donations);
        }

        public IActionResult Gallery()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var images = _context.GalleryImages.ToList();

            return View(images);
        }

        // GET: Admin/AddGallery
        [HttpGet]
        public IActionResult AddGallery()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            return View();
        }


        // POST: Admin/AddGallery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGallery(
            GalleryImage image,
            IFormFile? imageFile)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            // Validate uploaded file.
            if (imageFile is null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please select an image to upload.");

                return View(image);
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "",
                    "The image cannot exceed 5 MB.");

                return View(image);
            }

            // Validate image title.
            if (string.IsNullOrWhiteSpace(image.Title))
            {
                ModelState.AddModelError(
                    "Title",
                    "Please enter an image title.");
            }
            else if (image.Title.Trim().Length > 120)
            {
                ModelState.AddModelError(
                    "Title",
                    "The image title cannot exceed 120 characters.");
            }


            // Validate image category.
            string[] allowedCategories =
            {
                "Community Outreach",
                "Education",
                "Food Support",
                "Healthcare",
                "Youth Development",
                "Family Support",
                "Events",
                "Other"
            };

            string? selectedCategory = image.Category?.Trim();

            if (string.IsNullOrWhiteSpace(selectedCategory) ||
                !allowedCategories.Contains(selectedCategory, StringComparer.Ordinal))
            {
                ModelState.AddModelError(
                    "Category",
                    "Please select a valid photograph category.");
            }


            // Continue validation.
            if (!ModelState.IsValid)
            {
                return View(image);
            }

            // The compiler now knows imageFile is not null.
            string? extension =
                await DetectImageExtensionAsync(imageFile);

            if (extension == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid image. Please upload a valid JPG, PNG or WebP image.");

                return View(image);
            }

            // Use a dedicated folder for administrator uploads.
            string webRoot = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
            }

            string uploadsFolder = Path.Combine(
                webRoot,
                "uploads",
                "gallery");

            Directory.CreateDirectory(uploadsFolder);

            // Generate our own filename. Never trust the uploaded filename.
            string fileName = $"{Guid.NewGuid():N}{extension}";

            string filePath = Path.Combine(uploadsFolder,fileName);
            image.ImageUrl = $"/uploads/gallery/{fileName}";
            image.Title = image.Title.Trim();

            image.Category = selectedCategory!;

            image.Description = string.IsNullOrWhiteSpace(image.Description)
                ? null
                : image.Description.Trim();

            image.DateUploaded = DateTime.Now;

            try
            {
                // Store the uploaded image.
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Save gallery record and audit record together.
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                _context.GalleryImages.Add(image);

                // Save first so EF Core generates the image ID.
                await _context.SaveChangesAsync();

                _auditService.Record(
                    action: "Uploaded",
                    entityType: "GalleryImage",
                    entityId: image.Id,
                    description: $"Gallery image #{image.Id} was uploaded."
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Gallery image uploaded successfully!";

                return RedirectToAction(nameof(Gallery));
            }
            catch
            {
                // Remove the uploaded file if the database operation fails.
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                throw;
            }

        }


        // Verify JPG, PNG or WebP file signatures.
        // This does not rely on the filename or browser MIME type.
        private static async Task<string?> DetectImageExtensionAsync(
            IFormFile file)
        {
            byte[] header = new byte[12];

            await using var stream = file.OpenReadStream();

            int bytesRead = await stream.ReadAsync(
                header, 0, header.Length);

            // JPEG
            if (bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
            {
                return ".jpg";
            }

            // PNG
            if (bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A)
            {
                return ".png";
            }

            // WebP
            if (bytesRead >= 12 &&
                header[0] == (byte)'R' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F' &&
                header[3] == (byte)'F' &&
                header[8] == (byte)'W' &&
                header[9] == (byte)'E' &&
                header[10] == (byte)'B' &&
                header[11] == (byte)'P')
            {
                return ".webp";
            }

            return null;
        }

        // GET: Admin/DeleteGallery/5
        public IActionResult DeleteGallery(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var image = _context.GalleryImages.Find(id);

            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // POST: Admin/DeleteGallery/5

        [HttpPost]
        [ActionName("DeleteGallery")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGalleryConfirmed(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var image = _context.GalleryImages.Find(id);

            if (image == null)
            {
                TempData["Error"] = "Gallery image not found.";
                return RedirectToAction(nameof(Gallery));
            }

            string imageUrl = image.ImageUrl;

            // Check whether another gallery record uses the same file.
            bool isUsedElsewhere = _context.GalleryImages
                .Any(x => x.Id != image.Id && x.ImageUrl == imageUrl);

            int imageId = image.Id;

            // Record the deletion in the audit trail.
            _context.GalleryImages.Remove(image);

            _auditService.Record(
                action: "Deleted",
                entityType: "GalleryImage",
                entityId: image.Id,
                description: $"Gallery image #{image.Id} was deleted."
                );

            // Save the deletion and audit log together.
            _context.SaveChanges();

            // Only physically remove files from our dedicated upload folder.
            // Existing website assets under /images are protected.
            if (!isUsedElsewhere &&
                imageUrl.StartsWith(
                    "/uploads/gallery/",
                    StringComparison.OrdinalIgnoreCase))
            {
                string webRoot = _environment.WebRootPath;

                if (string.IsNullOrWhiteSpace(webRoot))
                {
                    webRoot = Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot");
                }

                string uploadsFolder = Path.Combine(
                    webRoot,
                    "uploads",
                    "gallery");

                string fileName = Path.GetFileName(imageUrl);

                string filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (IOException)
                {
                    TempData["Success"] =
                        "Gallery record deleted, but the physical image file could not be removed.";

                    return RedirectToAction(nameof(Gallery));
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["Success"] =
                        "Gallery record deleted, but the physical image file could not be removed.";

                    return RedirectToAction(nameof(Gallery));
                }
            }

            TempData["Success"] = "Gallery image deleted successfully.";

            return RedirectToAction(nameof(Gallery));
        }


        // GET: Admin/Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn())
                return RedirectToAction(nameof(Index));

            if (!string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("PendingAdminChallengeId")))
            {
                return RedirectToAction(nameof(VerifyEmail));
            }

            return View();
        }

        [AllowAnonymous]
        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            CancellationToken cancellationToken)
        {
            if (IsLoggedIn())
                return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
                return View(model);

            var username = model.Username.Trim();

            var admin = await _context.AdminUsers
                .SingleOrDefaultAsync(
                    a => a.Username == username,
                    cancellationToken);

            if (admin == null ||
                string.IsNullOrWhiteSpace(admin.Password) ||
                !BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    admin.Password))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            // Remove any previous pending login state.
            HttpContext.Session.Clear();

            try
            {
                // Generate a challenge and email the verification code.
                Guid challengeId =
                    await _verificationService.CreateAndSendAsync(
                        admin,
                        cancellationToken);

                // Store the challenge in the server-side session.
                // No authenticated cookie is issued at this stage.
                HttpContext.Session.SetString(
                    "PendingAdminChallengeId",
                    challengeId.ToString("D"));

                return RedirectToAction(nameof(VerifyEmail));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Administrator verification email could not be sent.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not send your verification email. Please try again later.");

                return View(model);
            }
        }

        // GET: Admin/VerifyEmail
        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            if (IsLoggedIn())
                return RedirectToAction(nameof(Index));

            string? challenge =
                HttpContext.Session.GetString(
                    "PendingAdminChallengeId");

            if (!Guid.TryParse(challenge, out _))
                return RedirectToAction(nameof(Login));

            return View(new EmailVerificationViewModel());
        }

        // POST: Admin/VerifyEmail
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(
            EmailVerificationViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? challenge =
                HttpContext.Session.GetString(
                    "PendingAdminChallengeId");

            if (!Guid.TryParse(challenge, out Guid challengeId))
                return RedirectToAction(nameof(Login));

            var admin = await _verificationService.VerifyAsync(
                challengeId,
                model.Code,
                cancellationToken);

            if (admin == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The verification code is invalid, expired, or cannot be used. Please try again or request another code.");

                return View(model);
            }

            // Create the authenticated administrator identity.
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    admin.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    admin.Username),

                new Claim(
                    ClaimTypes.Email,
                    admin.Email),

                new Claim(
                    ClaimTypes.Role,
                    "Administrator")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            // Issue the authentication cookie only after email verification.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = false
                });

            // Remove temporary verification state.
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ResendVerificationCode
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(
            CancellationToken cancellationToken)
        {
            string? challenge =
                HttpContext.Session.GetString(
                    "PendingAdminChallengeId");

            if (!Guid.TryParse(challenge, out Guid challengeId))
                return RedirectToAction(nameof(Login));

            bool sent = await _verificationService.ResendAsync(
                challengeId,
                cancellationToken);

            TempData[sent ? "Success" : "Error"] = sent
                ? "A new verification code has been sent."
                : "The code could not be resent. It may be too early, expired, or have reached its resend limit. Please try signing in again if necessary.";

            return RedirectToAction(nameof(VerifyEmail));
        }

        // POST: Admin/CancelVerification
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelVerification()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }

        // Check authenticated administrator identity.
        private bool IsLoggedIn()
        {
            return User.Identity?.IsAuthenticated == true;
        }

        // POST: Admin/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMessage(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var message = _context.ContactMessages.Find(id);

            if (message == null)
            {
                TempData["Error"] = "Message could not be found.";
                return RedirectToAction(nameof(Messages));
            }

            // Remove the selected message.
            _context.ContactMessages.Remove(message);

            _auditService.Record
             (
                action: "Deleted",
                entityType: "ContactMessage",
                entityId: message.Id,
                description: $"Contact message #{message.Id} was deleted."
            );

            // Save both changes together.
            _context.SaveChanges();

            TempData["Success"] = "Message deleted successfully.";

            return RedirectToAction(nameof(Messages));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VoidDonation(int id, string reason)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            {
                TempData["Error"] =
                    "Please provide a valid reason (maximum 500 characters).";

                return RedirectToAction(nameof(Donations));
            }

            var donation = _context.Donations.Find(id);

            if (donation == null)
            {
                TempData["Error"] = "Donation record not found.";
                return RedirectToAction(nameof(Donations));
            }

            if (donation.IsVoided)
            {
                TempData["Error"] = "This donation has already been voided.";
                return RedirectToAction(nameof(Donations));
            }

            var adminUsername = User.Identity?.Name ?? "Unknown";

            var trimmedReason = reason.Trim();

            // Preserve the original donation record.
            donation.IsVoided = true;
            donation.VoidedAtUtc = DateTime.UtcNow;
            donation.VoidedBy = adminUsername;
            donation.VoidReason = trimmedReason;

            // Record the donation void in the audit history.
            _auditService.Record(
                action: "Voided",
                entityType: "Donation",
                entityId: donation.Id,
                description: $"Donation #{donation.Id} was voided.",
                reason: trimmedReason
            );

            // Save donation changes and audit record together.
            _context.SaveChanges();

            TempData["Success"] = "Donation record voided successfully.";

            return RedirectToAction(nameof(Donations));
        }

        // GET: Admin/AddAdmin
        [HttpGet]
        public IActionResult AddAdmin()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdmin(AdminUser model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                _context.AdminUsers.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Admin account created successfully!";

                return RedirectToAction(nameof(AddAdmin));
            }

            return View(model);
        }
        //Add a ViewAdmins action
        public IActionResult ViewAdmins()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var admins = _context.AdminUsers.ToList();

            return View(admins);
        }


        // GET: Admin/AuditLogs
        [HttpGet]
        public IActionResult AuditLogs()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction(nameof(Login));
            }

            var logs = _context.AuditLogs
                .OrderByDescending(a => a.CreatedAtUtc)
                .Take(100)
                .ToList();

            return View("AuditLogs", logs);
        }

    }
}