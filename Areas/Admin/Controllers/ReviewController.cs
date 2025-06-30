using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels;


namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews.Include(r => r.ReviewTranslations)
                .Include(r => r.User)
                .Select(r => new ReviewAdminVM
                {
                    Id = r.Id,
                    TourId = r.TourId,
                    UserName = r.User.UserName,
                    UserImage = r.User.Image ?? "ImagePP.webp",
                    Rating = r.Rating,
                    Comment = r.ReviewTranslations.FirstOrDefault().Comment,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id, bool isApproved)
        {
            var review = await _context.Reviews.Include(r => r.ReviewTranslations).FirstOrDefaultAsync(r => r.TourId == id);
            if (review == null)
                return NotFound();

            review.IsApproved = isApproved;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}


