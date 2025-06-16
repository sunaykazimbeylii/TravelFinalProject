
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
using TravelFinalProject.ViewModels;
[Authorize]
[Route("booking")]
public class BookingController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public BookingController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("create/{tourId?}")]
    public async Task<IActionResult> Create(int? tourId)
    {

        if (tourId == null || tourId < 1) return BadRequest();

        var tour = await _context.Tours
                     .Include(t => t.TourImages)
                     .Include(t => t.Destination)
                     .FirstOrDefaultAsync(t => t.Id == tourId);

        if (tour == null) return NotFound();

        var model = new BookingVM
        {
            Booking = new Booking
            {
                TourId = tourId.Value,
                GuestsCount = 1,
                TotalPrice = tour.Price ?? 0,
                Tour = tour
            },
            Travellers = new List<BookingTraveller> { new BookingTraveller() }
        };
        return View(model);
    }

    [HttpPost]
    [Route("booking/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVM bookingVM)
    {
        int tourId = bookingVM.Booking.TourId;
        string currentUsername = User.Identity?.Name;
        if (currentUsername == null) return Unauthorized();

        AppUser currentUser = await _userManager.FindByNameAsync(currentUsername);
        if (currentUser == null) return Unauthorized();

        var tour = await _context.Tours
            .Include(t => t.TourImages)
            .Include(t => t.Destination)
            .FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null) return NotFound();

        if (!ModelState.IsValid) return View(bookingVM);

        bookingVM.Booking.UserId = currentUser.Id;
        bookingVM.Booking.User = currentUser;
        bookingVM.Booking.TourId = tourId;
        bookingVM.Booking.Tour = tour;
        bookingVM.Booking.BookingDate = DateTime.UtcNow;
        bookingVM.Booking.Status = BookingStatus.Pending;

        _context.Bookings.Add(bookingVM.Booking);
        await _context.SaveChangesAsync();

        foreach (var traveller in bookingVM.Travellers)
        {
            traveller.BookingId = bookingVM.Booking.Id;
            _context.BookingTravellers.Add(traveller);
        }
        await _context.SaveChangesAsync();

        return Content("Success.");
    }
}
