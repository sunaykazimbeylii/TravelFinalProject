using Microsoft.AspNetCore.Mvc;
using TravelFinalProject.DAL;

namespace TravelFinalProject.Controllers
{
    public class TourController : Controller
    {
        private readonly AppDbContext _context;

        public TourController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tours = _context.Tours.ToList();
            return View(tours);
        }
    }
}
