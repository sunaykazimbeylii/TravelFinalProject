using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels;
using TravelFinalProject.ViewModels.TourVM;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TourController : Controller
    {
        private readonly AppDbContext _context;

        public TourController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<GetTourVM> tourVMs = await _context.Tours.Select(t => new GetTourVM
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Price = t.Price,
                Destination = t.Destination,
                DestinationId = t.DestinationId,
                Duration = t.Duration,
                Start_Date = t.Start_Date,
                End_Date = t.End_Date,
                Image = t.Image,
                Available_seats = t.Available_seats,
                Location = t.Location

            }).ToListAsync();

            return View(tourVMs);
        }
        public async Task<IActionResult> Create()
        {
            CreateTourVM tourVM = new CreateTourVM
            {
                Destinations = await _context.Destinations.ToListAsync()
            };
            return View(tourVM);
        }

    }
}
