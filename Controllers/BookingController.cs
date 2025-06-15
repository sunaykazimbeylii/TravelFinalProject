
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
[Authorize]
public class BookingController : Controller
{
    private readonly AppDbContext _context;

    public BookingController(AppDbContext context)
    {
        _context = context;
    }

    // 1. "Book Now" ilə istifadəçi form ilə qonaq sayını, tarixi təsdiq edib göndərir
    [HttpGet]
    public async Task<IActionResult> Book(int tourId)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null) return NotFound();

        ViewBag.Tour = tour;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tourId, int guestsCount)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null) return NotFound();

        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        Booking booking = new Booking
        {
            UserId = userId,
            TourId = tourId,
            BookingDate = DateTime.UtcNow,
            GuestsCount = guestsCount,
            TotalPrice = tour.Price.Value * guestsCount,
            Status = BookingStatus.Pending
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyBookings");
    }

    // 2. "Instant Book" ilə dərhal təsdiqləyir
    public async Task<IActionResult> InstantBook(int tourId)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null) return NotFound();

        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        Booking booking = new Booking
        {
            UserId = userId,
            TourId = tourId,
            BookingDate = DateTime.UtcNow,
            GuestsCount = 1,
            TotalPrice = tour.Price.Value,
            Status = BookingStatus.Confirmed
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyBookings");
    }

    // 3. istifadəçi ilə əlaqəli bookingleri gösterir
    //public async Task<IActionResult> MyBookings()
    //{
    //    int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    //    var myBookings = await _context.Bookings.ToListAsync();

    //        .Where(b => b.UserId == userId)


    //    return View(myBookings);
    //}
}