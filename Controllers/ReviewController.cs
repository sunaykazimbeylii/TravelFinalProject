using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.ViewModels.ReviewVM;

namespace TravelFinalProject.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ReviewAdd(int tourId)
        {

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == tourId);
            if (tour == null) return NotFound();

            var vm = new ReviewVM
            {
                TourId = tour.Id,
                TourTitle = tour.Title
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> ReviewAdd(ReviewVM vm)
        {
            if (!ModelState.IsValid)
            {
                var tour = await _context.Tours.FindAsync(vm.TourId);
                vm.TourTitle = tour?.Title ?? "";
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = new Review
            {
                UserId = userId,
                Comment = vm.Comment,
                Rating = vm.Rating,
                TourId = vm.TourId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Content("ThankYou");
        }


    }
}



