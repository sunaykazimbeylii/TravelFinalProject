using Microsoft.AspNetCore.Mvc;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TourController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
