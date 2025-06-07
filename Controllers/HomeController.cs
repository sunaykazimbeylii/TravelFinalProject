using Microsoft.AspNetCore.Mvc;

namespace TravelFinalProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
