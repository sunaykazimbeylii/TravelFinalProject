using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels;

namespace TravelFinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Tours = await _context.Tours.ToListAsync(),
                Slides = await _context.Slides.ToListAsync(),

            };
            return View(homeVM);
        }
    }
}
