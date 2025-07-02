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
        public async Task<IActionResult> Index(
         TourSearchVM search,
         int? destinationId,
         int? min_price,
         int? max_price,
         int page = 1,
         int key = 1,
         string langCode = "en")
        {
            string currencyCode = Request.Cookies["SelectedCurrency"] ?? "USD";
            int pageSize = 3;

            // Base query
            var query = _context.Tours
                .Include(t => t.TourTranslations)
                .Include(t => t.TourImages.Where(ti => ti.IsPrimary == true))
                .Include(t => t.Destination)
                    .ThenInclude(d => d.DestinationTranslations)
                .AsQueryable();

            // Dilə uyğun tərcüməsi olan turlar
            query = query.Where(t => t.TourTranslations.Any(tr => tr.LangCode == langCode));

            // DestinationId filtresi varsa
            if (destinationId.HasValue && destinationId > 0)
                query = query.Where(t => t.DestinationId == destinationId);

            // Qiymət aralığı filtri
            if (min_price.HasValue)
                query = query.Where(t => t.Price >= min_price.Value);

            if (max_price.HasValue)
                query = query.Where(t => t.Price <= max_price.Value);

            // Search.Destination uyğun dil və adla filtrləmə
            if (!string.IsNullOrEmpty(search.Destination))
            {
                query = query.Where(t =>
                    t.Destination.DestinationTranslations.Any(dt =>
                        dt.LangCode == langCode && dt.Name == search.Destination));
            }

            // Tarix filtrləri
            if (search.CheckIn.HasValue)
            {
                DateTime checkIn = search.CheckIn.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(t => t.Start_Date.ToDateTime(TimeOnly.MinValue) >= checkIn);
            }

            if (search.CheckOut.HasValue)
            {
                DateTime checkOut = search.CheckOut.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(t => t.End_Date.ToDateTime(TimeOnly.MaxValue) <= checkOut);
            }

            // Adam sayı filtresi
            if (search.Adults.HasValue)
                query = query.Where(t => t.Available_seats >= search.Adults);

            // Sıralama
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
                default:
                    query = query.OrderBy(t => t.Id);
                    break;
            }

            // Sayfalama
            int count = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1 || page > totalPages) return BadRequest();

            var pagedTours = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var tourVMs = new List<GetTourVM>();
            foreach (var t in pagedTours)
            {
                var translation = t.TourTranslations.FirstOrDefault(tr => tr.LangCode == langCode);
                var destinationTranslation = t.Destination?.DestinationTranslations.FirstOrDefault(dt => dt.LangCode == langCode);

                decimal originalPrice = t.Price ?? 0;
                decimal convertedPrice = await _currencyService.ConvertAsync(originalPrice, currencyCode);

                tourVMs.Add(new GetTourVM
                {
                    Id = t.Id,
                    Title = translation?.Title ?? "No title",
                    DestinationId = t.DestinationId,
                    Start_Date = t.Start_Date,
                    End_Date = t.End_Date,
                    Location = translation?.Location,
                    Available_seats = t.Available_seats,
                    Image = t.TourImages.FirstOrDefault()?.Image ?? "default.jpg",
                    Description = translation?.Description,
                    DestinationName = destinationTranslation?.Name,
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

            // Dilə uyğun destinationlar (siyahı üçün)
            var destinations = await _context.Destinations
                .Include(d => d.DestinationTranslations.Where(dt => dt.LangCode == langCode))
                .ToListAsync();

            var vm = new TourListPageVM
            {
                PaginatedTours = paginatedVM,
                Destinations = destinations,
                SearchForm = new TourSearchVM()
            };

            return View(vm);
        }





        public async Task<IActionResult> TourDetail(int? id, string langCode = "en")
        {
            if (id is null || id < 1) return BadRequest();

            Tour tour = await _context.Tours.Include(t => t.TourTranslations.Where(tt => tt.LangCode == langCode))
                .Include(t => t.Destination).ThenInclude(d => d.DestinationTranslations)
                .Include(t => t.TourImages)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();

            decimal originalPrice = tour.Price ?? 0m;
            string currencyCode = Request.Cookies["SelectedCurrency"] ?? "USD";
            decimal convertedPrice = await _currencyService.ConvertAsync(originalPrice, currencyCode);
            string currencySymbol = _currencyService.GetSymbol(currencyCode);

            TourDetailVM detailVM = new TourDetailVM
            {
                Tour = tour,
                ConvertedPrice = convertedPrice,
                CurrencySymbol = currencySymbol,
                RelatedTour = await _context.Tours.Include(t => t.TourTranslations.Where(tt => tt.LangCode == langCode))
                 .Where(t => t.DestinationId == tour.DestinationId && t.Id != id)
                .Include(t => t.TourImages)
                .ToListAsync(),
            };

            return View(detailVM);
        }





    }
}
