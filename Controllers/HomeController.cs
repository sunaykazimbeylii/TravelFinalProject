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


        public async Task<IActionResult> Index(int? categoryId, string langCode = "en")
        {
            if (!string.IsNullOrEmpty(langCode))
            {
                Response.Cookies.Append("langCode", langCode, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(1)
                });
            }
            else
            {
                langCode = Request.Cookies["langCode"];
                if (string.IsNullOrEmpty(langCode))
                {
                    langCode = "en";
                }
            }
            string currencyCode = Request.Cookies["currency"] ?? "USD";
            var destinationsQuery = _context.Destinations
                .Include(m => m.DestinationTranslations.Where(t => t.LangCode == langCode))
                .Include(d => d.Category)
                .Where(d => d.IsFeatured)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                destinationsQuery = destinationsQuery.Where(d => d.CategoryId == categoryId);
            }

            var destinations = await destinationsQuery.Where(t => t.DestinationTranslations.Any(tt => tt.LangCode == langCode))
                    .Include(t => t.DestinationTranslations.Where(tt => tt.LangCode == langCode)).Take(3)

    .ToListAsync();

            HomeVM homeVM = new HomeVM
            {
                Tours = await _context.Tours
                    .Where(t => t.TourTranslations.Any(tt => tt.LangCode == langCode))
                    .Include(t => t.TourTranslations.Where(tt => tt.LangCode == langCode)).Take(3)
                    .ToListAsync(),

                Slides = await _context.Slides
                    .Include(m => m.SlideTranslations.Where(t => t.LangCode == langCode))
                    .ToListAsync(),

                DestinationCategories = await _context.DestinationCategories
                    .Include(m => m.DestinationCategoryTranslations.Where(t => t.LangCode == langCode))
                    .ToListAsync(),

                DestinationImages = await _context.DestinationImages.ToListAsync(),

                Reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.ReviewTranslations)
                    .Where(r => r.IsApproved && r.ReviewTranslations.Any(rt => rt.LangCode == langCode)).Take(3)
                    .ToListAsync(),

                TourImages = await _context.TourImages.ToListAsync(),

                Destinations = destinations,
                CurrentCategoryId = categoryId,
            };

            return View(homeVM);
        }


        public async Task<IActionResult> DestinationDetails(int? id, string langCode = "en")
        {
            if (id is null || id < 1) return BadRequest();
            var destination = await _context.Destinations.Include(d => d.DestinationTranslations.Where(d => d.LangCode == langCode))
                .Include(d => d.Category)
                .Include(d => d.DestinationImages)
                .Include(d => d.Tours)


                .FirstOrDefaultAsync(d => d.Id == id);

            if (destination == null) return NotFound();
            DestinationDetailVM detailVM = new DestinationDetailVM
            {
                Destination = destination,
                RelatedDestinations = await _context.Destinations
                .Where(d => d.CategoryId == destination.CategoryId && d.Id != destination.Id)
                .Include(d => d.DestinationImages)
                .ToListAsync(),

            };
            return View(detailVM);
        }
        //public async Task<IActionResult> Error(string errorMesage)
        //{
        //    return View(model: errorMesage);
        //}
    }
}
