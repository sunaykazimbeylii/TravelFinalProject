using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.ViewModels;

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
            var tours = _context.Tours.Include(t => t.Destination).ToList();
            return View(tours);
        }
        public async Task<IActionResult> TourDetail(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Tour tour = await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.TourImages)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();
            TourDetailVM detailVM = new TourDetailVM
            {
                Tour = tour,
                RelatedTour = await _context.Tours
                 .Where(t => t.DestinationId == tour.DestinationId && t.Id != id)
                .Include(t => t.TourImages)
                .ToListAsync(),

            };
            return View(detailVM);
        }
    }
}
