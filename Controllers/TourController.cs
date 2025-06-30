using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Interfaces;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities.Enums;
using TravelFinalProject.ViewModels;

namespace TravelFinalProject.Controllers
{
    public class TourController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyService _currencyService;

        public TourController(AppDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }
        public async Task<IActionResult> Index(TourSearchVM search, int? destinationId, int? min_price, int? max_price, int page = 1, int key = 1, string langCode = "en")
        {
            IQueryable<Tour> query = _context.Tours
                .Include(m => m.TourTranslations.Where(t => t.LangCode == langCode))
                .Include(t => t.TourImages.Where(ti => ti.IsPrimary == true))
                .Include(t => t.Destination).ThenInclude(t => t.DestinationTranslations);

            if (destinationId != null && destinationId > 0)
                query = query.Where(t => t.DestinationId == destinationId);

            if (min_price.HasValue)
                query = query.Where(t => t.Price >= min_price.Value);

            if (max_price.HasValue)
                query = query.Where(t => t.Price <= max_price.Value);

            if (!string.IsNullOrEmpty(search.Destination))
                query = query.Where(t => t.Destination.DestinationTranslations.FirstOrDefault().Name == search.Destination);

            if (search.CheckOut.HasValue)
            {
                DateTime checkOut = search.CheckOut.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(t => t.End_Date.ToDateTime(TimeOnly.MaxValue) <= checkOut);
            }

            if (search.CheckIn.HasValue)
            {
                DateTime checkIn = search.CheckIn.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(t => t.Start_Date.ToDateTime(TimeOnly.MinValue) >= checkIn);
            }

            if (search.Adults.HasValue)
                query = query.Where(t => t.Available_seats >= search.Adults);

            switch (key)
            {
                case (int)SortType.Price:
                    query = query.OrderBy(t => t.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case (int)SortType.Duration:
                    query = query.OrderByDescending(t => t.Duration);
                    break;
            }

            int count = await query.CountAsync();
            int pageSize = 3;
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1 || page > totalPages) return BadRequest();

            var tours = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            string currencyCode = Request.Cookies["SelectedCurrency"] ?? "USD";

            var tourVMs = new List<GetTourVM>();
            foreach (var t in tours)
            {
                decimal originalPrice = t.Price ?? 0;
                decimal convertedPrice = await _currencyService.ConvertAsync(originalPrice, currencyCode);

                tourVMs.Add(new GetTourVM
                {
                    Id = t.Id,
                    Title = t.TourTranslations.FirstOrDefault()?.Title,
                    DestinationId = t.DestinationId,
                    Start_Date = t.Start_Date,
                    End_Date = t.End_Date,
                    Location = t.TourTranslations.FirstOrDefault()?.Location,
                    Available_seats = t.Available_seats,
                    Image = t.TourImages.FirstOrDefault()?.Image ?? "default.jpg",
                    Description = t.TourTranslations.FirstOrDefault()?.Description,
                    Destination = t.Destination,
                    Price = convertedPrice,
                    CurrencyCode = currencyCode
                });
            }

            var paginatedVM = new PaginatedVM<GetTourVM>
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = tourVMs
            };

            var destinations = await _context.Destinations
                .Include(m => m.DestinationTranslations.Where(t => t.LangCode == langCode))
                .ToListAsync();

            var vm = new TourListPageVM
            {
                PaginatedTours = paginatedVM,
                Destinations = destinations,
                SearchForm = new TourSearchVM()
            };

            return View(vm);
        }

        public async Task<IActionResult> TourDetail(int? id, string selectedCurrency = "USD")
        {
            if (id is null || id < 1) return BadRequest();

            Tour tour = await _context.Tours.Include(t => t.TourTranslations)
                .Include(t => t.Destination).ThenInclude(d => d.DestinationTranslations)
                .Include(t => t.TourImages)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();
            decimal price = tour.Price.Value;
            decimal convertedPrice = await _currencyService.ConvertAsync(price, selectedCurrency);
            string currencySymbol = _currencyService.GetSymbol(selectedCurrency);
            TourDetailVM detailVM = new TourDetailVM
            {
                Tour = tour,
                ConvertedPrice = convertedPrice,
                CurrencySymbol = currencySymbol,
                RelatedTour = await _context.Tours.Include(t => t.TourTranslations)
                 .Where(t => t.DestinationId == tour.DestinationId && t.Id != id)
                .Include(t => t.TourImages)
                .ToListAsync(),

            };
            return View(detailVM);
        }

        //[HttpPost]
        //public async Task<IActionResult> SearchTour(TourSearchVM vm)
        //{
        //    var query = _context.Tours.Include(t => t.Destination).AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(vm.Tour))
        //        query = query.Where(t => t.Tour != null && t.Destination.Name.ToLower().Contains(vm.D.ToLower()));

        //    if (vm.CheckIn.HasValue)
        //        query = query.Where(t => t.Start_Date >= vm.CheckIn.Value);

        //    if (vm.CheckOut.HasValue)
        //        query = query.Where(t => t.End_Date <= vm.CheckOut.Value);

        //    if (vm.Adults.HasValue)
        //        query = query.Where(t => t.Available_seats >= vm.Adults.Value);

        //    var firstTour = await query.FirstOrDefaultAsync();

        //    if (firstTour == null)
        //    {
        //        ModelState.AddModelError("", "No tours found matching your criteria.");
        //        vm.Results = new List<Tour>();
        //        return View(vm);
        //    }

        //    return RedirectToAction("Create", "Booking", new { adults = vm.Adults, children = vm.Children });


        //}


    }
}
