using Microsoft.AspNetCore.Mvc;

namespace ImpiloYesizweProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
