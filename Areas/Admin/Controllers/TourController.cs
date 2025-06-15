using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
using TravelFinalProject.ViewModels;
using TravelFinalProject.ViewModels.TourVM;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TourController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TourController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<GetTourVM> tourVMs = await _context.Tours.Select(t => new GetTourVM
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Price = t.Price.Value,
                Destination = t.Destination,
                DestinationId = t.DestinationId,
                Duration = t.Duration,
                Start_Date = t.Start_Date,
                End_Date = t.End_Date,
                Image = t.TourImages.FirstOrDefault(ti => ti.IsPrimary == true).Image,
                Available_seats = t.Available_seats,
                Location = t.Location

            }).ToListAsync();

            return View(tourVMs);
        }
        public async Task<IActionResult> Create()
        {
            CreateTourVM tourVM = new CreateTourVM
            {
                Destinations = await _context.Destinations.ToListAsync(),

            };
            return View(tourVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTourVM tourVM)

        {
            tourVM.Destinations = await _context.Destinations.ToListAsync();

            if (!ModelState.IsValid) return View(tourVM);
            if (!tourVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateTourVM.Photo), "Photo type is wrong.");
                return View(tourVM);
            }
            if (!tourVM.Photo.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(CreateTourVM.Photo), "File size cannot exceed 2 MB.");
                return View(tourVM);
            }
            bool Exist = await _context.Tours.AnyAsync(t => t.Title == tourVM.Title);
            if (Exist)
            {
                ModelState.AddModelError(nameof(CreateTourVM.Title), "This tour title already exists.");
                return View(tourVM);
            }
            if (!await _context.Destinations.AnyAsync(d => d.Id == tourVM.DestinationId))
            {
                ModelState.AddModelError(nameof(CreateTourVM.DestinationId), "Selected destination is invalid.");
                return View(tourVM);
            }
            if (tourVM.End_Date < tourVM.Start_Date)
            {
                ModelState.AddModelError(nameof(CreateTourVM.End_Date), "End date cannot be earlier than the start date.");
                return View(tourVM);
            }

            if (tourVM.Start_Date < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                ModelState.AddModelError(nameof(CreateTourVM.Start_Date), "Start date cannot be in the past.");
                return View(tourVM);
            }
            if (tourVM.Start_Date < DateOnly.FromDateTime(DateTime.Now.Date))
            {
                ModelState.AddModelError(nameof(CreateTourVM.Start_Date), "Start date cannot be in the past.");
            }
            bool overlap = await _context.Tours.AnyAsync(t =>
                t.DestinationId == tourVM.DestinationId &&
                t.Start_Date <= tourVM.End_Date &&
                t.End_Date >= tourVM.Start_Date
            );
            if (overlap)
            {
                ModelState.AddModelError("", "Another tour exists to the same destination during the selected period.");
                return View(tourVM);
            }
            bool duplicateTour = await _context.Tours.AnyAsync(t =>
            t.Start_Date == tourVM.Start_Date &&
            t.Location.ToLower() == tourVM.Location.ToLower()
        );
            if (duplicateTour)
            {
                ModelState.AddModelError("", "Another tour is already planned for this date and location.");
                return View(tourVM);
            }

            TourImage main = new TourImage
            {
                Image = await tourVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "trending"),
                IsPrimary = true,
                CreatedAt = DateTime.Now
            };
            Tour tour = new Tour
            {
                Title = tourVM.Title,
                Description = tourVM.Description,
                Price = tourVM.Price,
                Duration = tourVM.Duration,
                Start_Date = tourVM.Start_Date,
                End_Date = tourVM.End_Date,
                Available_seats = tourVM.Available_seats.Value,
                Location = tourVM.Location,
                TourImages = new List<TourImage>(),
                DestinationId = tourVM.DestinationId,
                CreatedAt = DateTime.Now

            };
            tour.TourImages.Add(main);
            await _context.Tours.AddAsync(tour);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int id)
        {
            Tour? tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id);
            if (tour is null) return NotFound();

            var tourVM = new UpdateTourVM
            {
                Title = tour.Title,
                Description = tour.Description,
                Price = tour.Price,
                Duration = tour.Duration,
                Start_Date = tour.Start_Date,
                End_Date = tour.End_Date,
                Available_seats = tour.Available_seats,
                Location = tour.Location,
                DestinationId = tour.DestinationId,
                Destinations = await _context.Destinations.ToListAsync(),
                Image = tour.TourImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
            };

            return View(tourVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateTourVM tourVM, int id)
        {
            tourVM.Destinations = await _context.Destinations.ToListAsync();

            if (!ModelState.IsValid) return View(tourVM);

            Tour? existTour = await _context.Tours.Include(t => t.TourImages).FirstOrDefaultAsync(t => t.Id == id);
            if (existTour is null) return NotFound();

            if (!await _context.Destinations.AnyAsync(d => d.Id == tourVM.DestinationId))
            {
                ModelState.AddModelError(nameof(UpdateTourVM.DestinationId), "Selected destination is invalid.");
                return View(tourVM);
            }

            if (tourVM.End_Date < tourVM.Start_Date)
            {
                ModelState.AddModelError(nameof(UpdateTourVM.End_Date), "End date cannot be earlier than start date.");
                return View(tourVM);
            }

            if (tourVM.Start_Date < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                ModelState.AddModelError(nameof(UpdateTourVM.Start_Date), "Start date cannot be in the past.");
                return View(tourVM);
            }

            if (tourVM.Photo != null)
            {
                if (!tourVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateTourVM.Photo), "Invalid image type.");
                    return View(tourVM);
                }

                if (!tourVM.Photo.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(UpdateTourVM.Photo), "Image size cannot exceed 2MB.");
                    return View(tourVM);
                }



            }
            if (tourVM.Photo != null)
            {
                TourImage main = new TourImage
                {
                    IsPrimary = true,
                    Image = await tourVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "trending"),
                    CreatedAt = DateTime.Now
                };
                TourImage? exitedMain = existTour.TourImages.FirstOrDefault(p => p.IsPrimary == true);
                exitedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "trending");
                existTour.TourImages.Remove(exitedMain);
                existTour.TourImages.Add(main);
            }
            existTour.Title = tourVM.Title;
            existTour.Description = tourVM.Description;
            existTour.Price = tourVM.Price;
            existTour.Duration = tourVM.Duration;
            existTour.Start_Date = tourVM.Start_Date;
            existTour.End_Date = tourVM.End_Date;
            existTour.Available_seats = tourVM.Available_seats ?? 0;
            existTour.Location = tourVM.Location;
            existTour.DestinationId = tourVM.DestinationId;
            existTour.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();
            Tour? tour = await _context.Tours.Include(t => t.TourImages).FirstOrDefaultAsync(s => s.Id == id);
            if (tour is null) return NotFound();
            foreach (TourImage TImage in tour.TourImages)
            {
                TImage.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            };
            _context.Remove(tour);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
