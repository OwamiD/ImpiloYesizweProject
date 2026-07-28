using Microsoft.AspNetCore.Mvc;
using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using ImpiloYesizweProject.ViewModels;

namespace ImpiloYesizweProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Gallery()
        {
            return View();
        }

        // GET
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Donate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Donate(DonationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var donation = new Donation
                    {
                        DonorName = model.DonorName,
                        Email = model.Email,
                        Amount = model.Amount,
                        DonationDate = DateTime.Now
                    };

                    _context.Donations.Add(donation);
                    _context.SaveChanges();

                    TempData["Success"] = "Thank you for supporting ImpiloYesizwe!";
                    return RedirectToAction(nameof(Donate));
                }
                catch (Exception)
                {
                    TempData["Error"] = "Something went wrong while saving your donation. Please try again later.";
                    return View(model);
                }
            }

            return View(model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var message = new ContactMessage
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Message = model.Message,
                        DateSent = DateTime.Now
                    };

                    _context.ContactMessages.Add(message);
                    _context.SaveChanges();

                    TempData["Success"] = "Thank you! Your message has been sent successfully.";
                    return RedirectToAction(nameof(Contact));
                }
                catch (Exception)
                {
                    TempData["Error"] = "Something went wrong. Please try again later.";
                    return View(model);
                }
            }

            return View(model);
        }

    }
}