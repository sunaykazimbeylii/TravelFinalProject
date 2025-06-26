
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
    public async Task<IActionResult> Create(int? tourId, int adults = 1, int children = 0)
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
                Tour = tour,

            },
            Adults = adults,
            Children = children,
            //        TravellerCount = await _context.BookingTravellers
            //.Where(bt => bt.BookingId == booking.Id)
            //.CountAsync()

        };
        return View(model);
    }
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVM bookingVM)
    {
        if (bookingVM.Booking == null || bookingVM.Booking.TourId < 1)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            bookingVM.Booking.Tour = await _context.Tours
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
            GuestsCount = bookingVM.Adults + bookingVM.Children
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
        var booking = await _context.Bookings
            .Include(b => b.Tour)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    [HttpGet]
    public async Task<IActionResult> BookNow(int tourId)
    {
        var tour = await _context.Tours
            .Include(t => t.TourImages)
            .Include(t => t.Destination)
            .FirstOrDefaultAsync(t => t.Id == tourId);

        if (tour == null) return NotFound();

        var vm = new BookingVM
        {
            TourId = tourId,
            TotalPrice = tour.Price ?? 0
        };

        return View(vm);
    }
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> BookNow(BookingVM vm)
    //{
    //    if (!ModelState.IsValid) return View(vm);

    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // İstifadəçi ID-si

    //    var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == vm.TourId);
    //    if (tour == null) return NotFound();

    //    var booking = new Booking
    //    {
    //        UserId = userId,
    //        TourId = vm.TourId,
    //        BookingDate = DateTime.Now,
    //        GuestsCount = vm.TravellerCount,
    //        TotalPrice = vm.TotalPrice,
    //        Status = BookingStatus.Pending
    //    };

    //    _context.Bookings.Add(booking);
    //    await _context.SaveChangesAsync();

    //    return RedirectToAction("Success");
    //}



}
