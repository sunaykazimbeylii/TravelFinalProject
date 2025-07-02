using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.ViewModels.BlogVM;

namespace TravelFinalProject.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Blog(string langCode = "en")
        {
            var blogs = await _context.Blogs
                .Include(b => b.BlogTranslations.Where(t => t.LangCode == langCode))
                .ToListAsync();

            var model = blogs.Select(b => new BlogVM
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                PublishedDate = b.PublishedDate,
                Title = b.BlogTranslations.FirstOrDefault()?.Title,
                Content = b.BlogTranslations.FirstOrDefault()?.Content
            }).ToList();

            return View(model);
        }
    }
}
