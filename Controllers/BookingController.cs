
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

    public BookingController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    [HttpGet("create/{tourId?}")]
    public IActionResult Create(int tourId, int adults = 1, int children = 0)
    {
        var tour = _context.Tours.Include(t => t.TourImages).Include(t => t.Destination).FirstOrDefault(t => t.Id == tourId);
        if (tour == null)
            return NotFound();

        int travellerCount = adults + children;

        decimal basePrice = tour.Price.Value * travellerCount;
        decimal totalPrice = basePrice + 10 - 15; // misal booking fee + endirim

        var model = new BookingVM
        {
            TourId = tourId,
            Adults = adults,
            Children = children,
            TotalPrice = totalPrice,
            PassportNumbers = new List<string>(new string[travellerCount]),
            Booking = new Booking
            {
                TourId = tourId,
                GuestsCount = 1,
                TotalPrice = tour.Price ?? 0,
                Tour = tour,

            },
        };

        return View(model);
    }


    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVM bookingVM)
    {
        // ModelState-dən UserId yoxlamasını çıxarırıq, çünki formda yoxdur
        ModelState.Remove("Booking.UserId");

        if (bookingVM.Booking == null || bookingVM.Booking.TourId < 1)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            // Tour məlumatını yenidən doldur
            bookingVM.Booking.Tour = await _context.Tours.Include(t => t.TourImages)
                .Include(t => t.Destination)
                .FirstOrDefaultAsync(t => t.Id == bookingVM.Booking.TourId);
            return View(bookingVM);
        }

        var tour = await _context.Tours
            .Include(t => t.TourImages)
            .Include(t => t.Destination)
            .FirstOrDefaultAsync(t => t.Id == bookingVM.Booking.TourId);

        if (tour == null)
        {
            ModelState.AddModelError("", "Seçdiyiniz tur tapılmadı.");
            return View(bookingVM);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Booking booking = new()
        {
            TotalPrice = bookingVM.TotalPrice,
            Status = BookingStatus.Pending,
            UserId = currentUser.Id,
            User = currentUser,
            Tour = tour,
            GuestsCount = bookingVM.Adults + bookingVM.Children,
            TourId = tour.Id,
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        if (bookingVM.PassportNumbers != null && bookingVM.PassportNumbers.Any())
        {
            foreach (var passport in bookingVM.PassportNumbers)
            {
                if (!string.IsNullOrWhiteSpace(passport))
                {
                    var traveller = new BookingTraveller
                    {
                        BookingId = booking.Id,
                        PassportNumber = passport.Trim()
                    };

                    _context.BookingTravellers.Add(traveller);
                }
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BookingConfirmation", new { id = booking.Id });
    }


    [HttpGet]
    public async Task<IActionResult> BookingConfirmation(int id)
    {
        var booking = await _context.Bookings.Include(b => b.Travellers)
            .Include(b => b.Tour).ThenInclude(t => t.TourImages)
            .Include(b => b.Tour).ThenInclude(t => t.Destination)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        return View(booking); // View @model Booking olacaq
    }



}
