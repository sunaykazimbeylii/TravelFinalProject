using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels.BlogVM;

namespace TravelFinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string langCode = "en")
        {
            List<BlogVM> blogVMs = await _context.Blogs.Include(b => b.BlogTranslations.Where(bt => bt.LangCode == langCode)).Select(b => new BlogVM
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                PublishedDate = b.PublishedDate,
                Content = b.BlogTranslations.FirstOrDefault().Content,
                IsPopular = b.IsPopular,
                Title = b.BlogTranslations.FirstOrDefault().Title
            }).ToListAsync();

            return View(blogVMs);
        }
    }
}
