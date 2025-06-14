using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
using TravelFinalProject.ViewModels;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DestinationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DestinationController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<GetDestinationVM> destinationVMs = await _context.Destinations.Include(d => d.Category).Select(d => new GetDestinationVM
            {
                Name = d.Name,
                Id = d.Id,
                Description = d.Description,
                Address = d.Address,
                City = d.City,
                Country = d.Country,
                CategoryName = d.Category.Name,
                Price = d.Price,
                MainImage = d.DestinationImages.FirstOrDefault(di => di.IsPrimary == true).Image,

            }).ToListAsync();
            return View(destinationVMs);
        }



        public async Task<IActionResult> Create()
        {
            CreateDestinationVM destinationVM = new CreateDestinationVM
            {
                Categories = await _context.DestinationCategories.ToListAsync()
            };
            return View(destinationVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDestinationVM destinationVM)
        {
            if (!ModelState.IsValid) return View(destinationVM);


            if (!destinationVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateDestinationVM.MainPhoto), "Photo type is wrong.");
                return View(destinationVM);
            }
            if (!destinationVM.MainPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(CreateDestinationVM.MainPhoto), "File size cannot exceed 2 MB.");
                return View(destinationVM);
            }
            bool exists = await _context.Destinations.AnyAsync(d => d.Name == destinationVM.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(CreateDestinationVM.Name), "Destination with this name already exists.");
                return View(destinationVM);
            }

            DestinationImage main = new DestinationImage
            {
                Image = await destinationVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "trending"),
                IsPrimary = true,
                CreatedAt = DateTime.Now
            };
            Destination destination = new Destination
            {
                Name = destinationVM.Name,
                Description = destinationVM.Description,
                Country = destinationVM.Country,
                City = destinationVM.City,
                Price = destinationVM.Price,
                Address = destinationVM.Address,

                CategoryId = destinationVM.CategoryId,
                DestinationImages = new List<DestinationImage>()

            };
            destination.DestinationImages.Add(main);
            await _context.Destinations.AddAsync(destination);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Destination? destination = await _context.Destinations.Include(d => d.DestinationImages).FirstOrDefaultAsync(d => d.Id == id);
            if (destination == null) return NotFound();

            UpdateDestinationVM destinationVM = new UpdateDestinationVM
            {

                Name = destination.Name,
                Description = destination.Description,
                Country = destination.Country,
                CategoryId = destination.CategoryId,
                Address = destination.Address,
                City = destination.City,
                Price = destination.Price,
                IsFeatured = destination.IsFeatured,
                PrimaryImage = destination.DestinationImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                Categories = await _context.DestinationCategories.ToListAsync()
            };
            return View(destinationVM);


        }


        [HttpPost]

        public async Task<IActionResult> Update(UpdateDestinationVM destinationVM, int id)
        {
            destinationVM.Categories = await _context.DestinationCategories.ToListAsync();
            if (!ModelState.IsValid) return View(destinationVM);

            Destination? existed = await _context.Destinations.Include(d => d.DestinationImages).FirstOrDefaultAsync(d => d.Id == id);
            if (existed is null) return NotFound();
            if (destinationVM.MainPhoto is not null)
            {
                if (!destinationVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(CreateDestinationVM.MainPhoto), "Photo type is wrong.");
                    return View(destinationVM);
                }
                if (!destinationVM.MainPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(CreateDestinationVM.MainPhoto), "File size cannot exceed 2 MB.");
                    return View(destinationVM);
                }

            }
            bool result = _context.DestinationCategories.Any(c => c.Id == destinationVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(UpdateDestinationVM.CategoryId), "Category doesn't exist");
                return View(destinationVM);
            }
            bool nameExists = await _context.Destinations.AnyAsync(d => d.Id != id && d.Name == destinationVM.Name);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(UpdateDestinationVM.Name), "Bu adla başqa destination artıq mövcuddur.");
                return View(destinationVM);
            }
            if (destinationVM.MainPhoto != null)
            {
                DestinationImage main = new DestinationImage
                {
                    IsPrimary = true,
                    Image = await destinationVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "trending"),
                    CreatedAt = DateTime.Now
                };
                DestinationImage? exitedMain = existed.DestinationImages.FirstOrDefault(p => p.IsPrimary == true);
                exitedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "trending");
                existed.DestinationImages.Remove(exitedMain);
                existed.DestinationImages.Add(main);
            }
            existed.Name = destinationVM.Name;
            existed.Description = destinationVM.Description;
            existed.Country = destinationVM.Country;
            existed.Price = destinationVM.Price;
            existed.Address = destinationVM.Address;
            existed.City = destinationVM.City;

            existed.CategoryId = destinationVM.CategoryId;
            existed.IsFeatured = destinationVM.IsFeatured;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1)
            {
                return BadRequest();
            }
            Destination? destination = await _context.Destinations.Include(d => d.DestinationImages).FirstOrDefaultAsync(d => d.Id == id);
            if (destination is null) return NotFound();
            foreach (DestinationImage desImage in destination.DestinationImages)
            {
                desImage.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            }
            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

//slide,destinition.login register