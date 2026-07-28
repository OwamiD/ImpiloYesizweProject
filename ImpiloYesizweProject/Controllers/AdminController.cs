using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using ImpiloYesizweProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;


namespace ImpiloYesizweProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var model = new DashboardViewModel
            {
                TotalMessages = _context.ContactMessages.Count(),
                TotalDonations = _context.Donations.Count(),
                TotalGalleryImages = _context.GalleryImages.Count(),
                TotalServices = _context.Services.Count(),
                TotalDonationAmount = _context.Donations.Sum(d => d.Amount),
                RecentMessages = _context.ContactMessages

                    .OrderByDescending(x => x.DateSent)
                    .Take(5)
                    .ToList(),

                RecentDonations = _context.Donations
                    .OrderByDescending(x => x.DonationDate)
                    .Take(5)
                    .ToList()
            };

            return View(model);
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
        // GET: Admin/AddService
        public IActionResult AddService()
        {
            return View();
        }
        public IActionResult Services()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            var services = _context.Services.ToList();

            return View(services);
        }

        // POST: Admin/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddService(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                _context.SaveChanges();

                TempData["Success"] = "Service added successfully.";

                return RedirectToAction(nameof(Services));
            }

            return View(service);
        }
        // GET: Admin/EditService/5
        public IActionResult EditService(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            var service = _context.Services.Find(id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        // POST: Admin/EditService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Update(service);
                _context.SaveChanges();

                TempData["Success"] = "Service updated successfully.";

                return RedirectToAction(nameof(Services));
            }

            return View(service);
        }
        // GET: Admin/DeleteService/5
        public IActionResult DeleteService(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            var service = _context.Services.Find(id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Admin/DeleteService/5
        [HttpPost, ActionName("DeleteService")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteServiceConfirmed(int id)
        {
            var service = _context.Services.Find(id);

            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();

                TempData["Success"] = "Service deleted successfully.";
            }

            return RedirectToAction(nameof(Services));
        }
        public IActionResult Gallery()
        {
            var images = _context.GalleryImages.ToList();

            return View(images);
        }
        // GET
        public IActionResult AddGallery()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddGallery(GalleryImage image, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    image.ImageUrl = "/images/" + fileName;
                }

                image.DateUploaded = DateTime.Now;

                _context.GalleryImages.Add(image);
                _context.SaveChanges();

                TempData["Success"] = "Image uploaded successfully!";

                return RedirectToAction(nameof(Gallery));
            }

            return View(image);
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
        [HttpPost, ActionName("DeleteGallery")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGalleryConfirmed(int id)
        {
            var image = _context.GalleryImages.Find(id);

            if (image != null)
            {
                // Delete image file
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    string filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        image.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete database record
                _context.GalleryImages.Remove(image);
                _context.SaveChanges();

                TempData["Success"] = "Image deleted successfully.";
            }

            return RedirectToAction(nameof(Gallery));
        }
        // GET: Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Admin") != null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var admin = _context.AdminUsers.FirstOrDefault(a => a.Username == model.Username);

            if (admin != null &&
                BCrypt.Net.BCrypt.Verify(model.Password, admin.Password))
            {
                HttpContext.Session.SetString("Admin", admin.Username);

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Invalid username or password.");

            return View(model);
        }
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Admin") != null;
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
        public IActionResult DeleteMessage(int id)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var message = _context.ContactMessages.Find(id);

            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Messages));
        }
        public IActionResult DeleteDonation(int id)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var donation = _context.Donations.Find(id);

            if (donation != null)
            {
                _context.Donations.Remove(donation);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Donations));
        }
        //Create an Add Admin Page
        public IActionResult AddAdmin()
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdmin(AdminUser model)
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
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
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            var admins = _context.AdminUsers.ToList();

            return View(admins);
        }
    }
}