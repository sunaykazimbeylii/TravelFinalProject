
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Interfaces;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
using TravelFinalProject.ViewModels;
[Authorize]
[Route("booking")]
public class BookingController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService, ICurrencyService currencyService, ILogger<BookingController> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _currencyService = currencyService;
        _logger = logger;
    }

    [HttpGet("create/{tourId?}")]
    public async Task<IActionResult> Create(int tourId, int adults = 1, int children = 0, string langCode = "en")
    {
        if (adults < 1) adults = 1;
        if (children < 0) children = 0;

        var tour = await _context.Tours
            .Include(m => m.TourTranslations.Where(t => t.LangCode == langCode))
            .Include(t => t.TourImages)
            .Include(t => t.Destination).ThenInclude(t => t.DestinationTranslations)
            .FirstOrDefaultAsync(t => t.Id == tourId);

        if (tour == null)
            return NotFound();

        decimal originalPriceAdult = tour.Price ?? 0m;
        decimal originalPriceChild = originalPriceAdult * 0.5m;

        string currencyCode = Request.Cookies["SelectedCurrency"] ?? "USD";

        decimal pricePerAdult = await _currencyService.ConvertAsync(originalPriceAdult, currencyCode);
        decimal pricePerChild = await _currencyService.ConvertAsync(originalPriceChild, currencyCode);

        int guestsCount = adults + children;

        decimal promoDiscountPercent = 0;

        decimal totalPrice = adults * pricePerAdult + children * pricePerChild;
        totalPrice = totalPrice * (1 - promoDiscountPercent / 100);
        var guests = new List<BookingTravellerVM>();
        for (int i = 0; i < guestsCount; i++)
        {
            guests.Add(new BookingTravellerVM());
        }

        var model = new BookingVM
        {
            TourId = tourId,
            AdultsCount = adults,
            ChildrenCount = children,
            GuestsCount = guestsCount,
            PricePerAdult = pricePerAdult,
            PricePerChild = pricePerChild,
            PromoDiscountPercent = promoDiscountPercent,
            Guests = guests,
            Booking = new Booking
            {
                TourId = tourId,
                GuestsCount = guestsCount,
                TotalPrice = totalPrice,
                Tour = tour,
                CurrencyCode = currencyCode,
            }
        };

        return View(model);
    }



    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVM bookingVM)
    {

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();


        if (bookingVM.Guests == null || bookingVM.Guests.Count != bookingVM.GuestsCount)
        {
            ModelState.AddModelError("", "Bütün qonaqlar üçün məlumatları daxil edin.");
        }
        else
        {

            var hasEmptyPassport = bookingVM.Guests.Any(g => string.IsNullOrWhiteSpace(g.PassportNumber));
            if (hasEmptyPassport)
            {
                ModelState.AddModelError("", "Pasport nömrələri boş ola bilməz.");
            }

            var duplicatePassport = bookingVM.Guests
                .GroupBy(g => g.PassportNumber.Trim().ToLower())
                .Any(grp => grp.Count() > 1);

            if (duplicatePassport)
            {
                ModelState.AddModelError("", "Pasport nömrələri təkrarlana bilməz.");
            }


            if (string.IsNullOrWhiteSpace(bookingVM.Guests[0].Email))
            {
                ModelState.AddModelError("", "Əsas qonaq üçün Email daxil edin.");
            }
            if (string.IsNullOrWhiteSpace(bookingVM.Guests[0].PhoneNumber))
            {
                ModelState.AddModelError("", "Əsas qonaq üçün Telefon nömrəsi daxil edin.");
            }
        }

        if (!ModelState.IsValid)
        {

            var tour = await _context.Tours
                .Include(t => t.TourTranslations)
                .Include(t => t.TourImages)
                .Include(t => t.Destination).ThenInclude(d => d.DestinationTranslations)
                .FirstOrDefaultAsync(t => t.Id == bookingVM.TourId);

            bookingVM.Booking.Tour = tour;

            return View(bookingVM);
        }


        var tourForBooking = await _context.Tours.FindAsync(bookingVM.TourId);
        if (tourForBooking == null)
        {
            ModelState.AddModelError("", "Seçdiyiniz tur tapılmadı.");
            return View(bookingVM);
        }

        string currencyCode = Request.Cookies["SelectedCurrency"] ?? "USD";
        decimal originalPriceAdult = tourForBooking.Price ?? 0m;
        decimal originalPriceChild = originalPriceAdult * 0.5m;

        decimal pricePerAdult = await _currencyService.ConvertAsync(originalPriceAdult, currencyCode);
        decimal pricePerChild = await _currencyService.ConvertAsync(originalPriceChild, currencyCode);

        int adults = bookingVM.AdultsCount;
        int children = bookingVM.ChildrenCount;
        int guestsCount = adults + children;

        decimal subtotal = adults * pricePerAdult + children * pricePerChild;
        decimal discountAmount = bookingVM.PromoDiscountPercent > 0 ? subtotal * bookingVM.PromoDiscountPercent / 100 : 0;
        decimal finalTotal = subtotal - discountAmount;


        var booking = new Booking
        {
            TourId = tourForBooking.Id,
            UserId = currentUser.Id,
            GuestsCount = guestsCount,
            TotalPrice = finalTotal,
            Status = BookingStatus.Pending,
            PricePerAdult = pricePerAdult,
            PricePerChild = pricePerChild,
            PromoDiscountPercent = bookingVM.PromoDiscountPercent,
            CurrencyCode = currencyCode
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();


        foreach (var guest in bookingVM.Guests)
        {
            var traveller = new BookingTraveller
            {
                BookingId = booking.Id,
                DateOfBirth = guest.DateOfBirth.Value,
                PassportNumber = guest.PassportNumber.Trim(),
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                BookingTravellerTranslations = new List<BookingTravellerTranslation>
            {
                new BookingTravellerTranslation
                {
                    FirstName = guest.FirstName,
                    LastName = guest.LastName,
                    Gender = guest.Gender,
                    Nationality = guest.Nationality,
                    LangCode = "en"
                }
            }
            };

            _context.BookingTravellers.Add(traveller);
        }

        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(BookingConfirmation), new { id = booking.Id });
    }


    [HttpGet("confirmation/{id}")]
    public async Task<IActionResult> BookingConfirmation(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Tour).ThenInclude(b => b.TourTranslations)
            .Include(b => b.Travellers)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        var model = new BookingDetailVM
        {
            Booking = booking,
            Travellers = booking.Travellers.ToList()
        };

        return View(model);
    }
}




