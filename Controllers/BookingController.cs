
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
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVM bookingVM)
    {
        if (bookingVM.Booking == null || bookingVM.Booking.TourId < 1)
            return BadRequest();

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

        bookingVM.Booking.TotalPrice = tour.Price ?? 0;
        bookingVM.Booking.Status = BookingStatus.Pending;
        bookingVM.Booking.Tour = tour;
        bookingVM.Booking.UserId = currentUser.Id;
        bookingVM.Booking.User = currentUser;
        if (!ModelState.IsValid) return View(bookingVM);

        _context.Bookings.Add(bookingVM.Booking);
        await _context.SaveChangesAsync();
        string body = @" 
                      your order successfuly placed:
                     <table border="""">
                                <thead>
                                    <tr>
                                        <th>Name</th>                                        
                                        <th>Price</th>
                                        <th>Tour</th>
                                    </tr>
                                </thead>
                                <tbody>";
        foreach (var item in bookingVM.Bookings)
        {
            body += @$"
                                       <tr>
                                            <td>{item.User.Name}</td>
                                            <td>{item.TotalPrice}</td>                                           
                                            <td>{item.Tour.Title}</td>
                                        </tr>";
        }
        body += @"</tbody>
                                            </table>";


        if (bookingVM.Travellers != null && bookingVM.Travellers.Any())
        {
            foreach (var traveller in bookingVM.Travellers)
            {
                traveller.BookingId = bookingVM.Booking.Id;
                _context.BookingTravellers.Add(traveller);
            }

            await _context.SaveChangesAsync();
        }
        await _emailService.SendMailAsync(currentUser.Email, "Your Order", body, true);
        return RedirectToAction("BookingConfirmation", new { id = bookingVM.Booking.Id });
    }

    [HttpGet("confirmation")]
    public async Task<IActionResult> BookingConfirmation()
    {
        var booking = await _context.Bookings
        .Include(b => b.Tour).ToListAsync();
        return View(booking);
    }
}
