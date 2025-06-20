using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.ViewModels;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var bookings = _context.Bookings
                   .Include(b => b.Tour)
                   .Include(b => b.User)
                   .ToList();

            var travellers = _context.BookingTravellers
                .Where(t => bookings.Select(b => b.Id).Contains(t.BookingId))
                .ToList();

            var model = new BookingHistoryVM
            {
                Bookings = bookings,
                BookingTravellers = travellers,
                User = bookings.FirstOrDefault()?.User ?? new AppUser()
            };

            return View(model);
        }
    }
}
