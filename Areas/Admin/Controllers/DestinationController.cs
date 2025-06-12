using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DestinationController : Controller
    {
        private readonly AppDbContext _context;

        public DestinationController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetDestinationVM> destinationVMs = await _context.Destinations.Select(d => new GetDestinationVM
            {
                Name = d.Name,
                Id = d.Id,
                Description = d.Description,
                Image = d.Image,
                Country = d.Country
            }).ToListAsync();
            return View(destinationVMs);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateDestinationVM destinationVM)
        {
            if (!ModelState.IsValid) return View();


            return View();
        }
    }
}
//slide,destinition.login register