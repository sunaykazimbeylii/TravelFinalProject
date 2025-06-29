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
        public async Task<IActionResult> Index(int? categoryId, TourSearchVM search, string langCode = "en")
        {

            var destinationsQuery = _context.Destinations.Include(m => m.DestinationTranslations.Where(t => t.LangCode == langCode))
                .Include(d => d.Category)
                .Where(d => d.IsFeatured == true)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                destinationsQuery = destinationsQuery.Where(d => d.CategoryId == categoryId);
            }

            var destinations = await destinationsQuery.ToListAsync();


            var toursQuery = _context.Tours.AsQueryable();


            //if (!string.IsNullOrWhiteSpace(search?.Destination))
            //{
            //    var dest = search.Destination.Trim().ToLower();
            //    toursQuery = toursQuery.Where(t => t.Destination.DestinationTranslations..ToLower().Contains(dest));
            //}

            if (search.CheckIn.HasValue)
            {
                DateOnly checkInDate = search.CheckIn.Value;
                toursQuery = toursQuery.Where(t => t.Start_Date == checkInDate);
            }

            if (search.CheckOut.HasValue)
            {
                DateOnly checkOutDate = search.CheckOut.Value;
                toursQuery = toursQuery.Where(t => t.End_Date <= checkOutDate);
            }



            var tours = await toursQuery.ToListAsync();
            HomeVM homeVM = new HomeVM
            {
                Tours = tours,
                Slides = await _context.Slides.Include(m => m.SlideTranslations.Where(t => t.LangCode == langCode)).ToListAsync(),
                DestinationCategories = await _context.DestinationCategories.Include(m => m.DestinationCategoryTranslations.Where(t => t.LangCode == langCode)).ToListAsync(),
                DestinationImages = await _context.DestinationImages.ToListAsync(),
                TourImages = await _context.TourImages.ToListAsync(),
                Destinations = destinations,
                CurrentCategoryId = categoryId,
                Search = search
            };

            return View(homeVM);
        }

        //public async Task<IActionResult> Index(int? categoryId)
        //{

        //    var destinationsQuery = _context.Destinations
        //.Include(d => d.Category)
        //.Where(d => d.IsFeatured == true)
        //.AsQueryable();

        //    if (categoryId.HasValue && categoryId.Value > 0)
        //    {
        //        destinationsQuery = destinationsQuery.Where(d => d.CategoryId == categoryId);
        //    }
        //    var destinations = await destinationsQuery.ToListAsync();
        //    HomeVM homeVM = new HomeVM
        //    {
        //        Tours = await _context.Tours.ToListAsync(),
        //        Slides = await _context.Slides.ToListAsync(),
        //        DestinationCategories = await _context.DestinationCategories.ToListAsync(),
        //        DestinationImages = await _context.DestinationImages.ToListAsync(),
        //        TourImages = await _context.TourImages.ToListAsync(),

        //        //Destinations = await _context.Destinations.Where(d => d.IsFeatured == true).ToListAsync(),
        //        Destinations = destinations,
        //        CurrentCategoryId = categoryId

        //    };
        //    return View(homeVM);
        //}



        public async Task<IActionResult> DestinationDetails(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            var destination = await _context.Destinations
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

    }
}
