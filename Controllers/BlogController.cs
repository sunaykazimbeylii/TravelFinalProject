using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelFinalProject.DAL;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities.Enums;
using TravelFinalProject.ViewModels;
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

        public async Task<IActionResult> Blog(string langCode = "en", int page = 1, int key = 1)
        {
            int pageSize = 3;
            var query = _context.Blogs
                .Include(b => b.BlogTranslations.Where(t => t.LangCode == langCode))
                .AsQueryable();

            int count = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1 || page > totalPages) return BadRequest();

            var pagedBlogs = await query
                .OrderByDescending(b => b.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BlogVM
                {
                    Id = b.Id,
                    ImageUrl = b.ImageUrl,
                    PublishedDate = b.PublishedDate,
                    Title = b.BlogTranslations.FirstOrDefault().Title,
                    Content = b.BlogTranslations.FirstOrDefault().Content
                })
                .ToListAsync();
            switch (key)
            {

                case (int)SortType.Date:
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case (int)SortType.Rating:
                    query = query.OrderByDescending(t => t.IsPopular);
                    break;
                default:
                    query = query.OrderBy(t => t.Id);
                    break;
            }

            var paginatedVM = new PaginatedVM<BlogVM>
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = pagedBlogs
            };

            return View(paginatedVM);
        }
        public async Task<IActionResult> BlogDetail(int id, string langCode = "en")
        {
            var blog = await _context.Blogs
                .Include(b => b.BlogTranslations.Where(t => t.LangCode == langCode))
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.Replies)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return NotFound();

            var blogDetailVM = new BlogDetailVM
            {
                Id = blog.Id,
                ImageUrl = blog.ImageUrl,
                PublishedDate = blog.PublishedDate,
                Title = blog.BlogTranslations.FirstOrDefault()?.Title ?? "No Title",
                Comment = blog.BlogTranslations.FirstOrDefault()?.Content ?? "No Content",
                Reviews = blog.Reviews.Select(r => new BlogReviewVM
                {
                    Id = r.Id,
                    UserName = r.User?.UserName ?? "Unknown",
                    UserImage = r.User?.Image ?? "default.png",
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    Replies = r.Replies.Select(reply => new BlogReviewReplyVM
                    {
                        Id = reply.Id,
                        UserName = reply.UserName,
                        Comment = reply.Comment,
                        CreatedAt = reply.CreatedAt
                    }).ToList()
                }).ToList()
            };

            return View(blogDetailVM);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(BlogReviewCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("BlogDetail", new { id = model.BlogId });
            }

            var blog = await _context.Blogs.FindAsync(model.BlogId);
            if (blog == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = new BlogReview
            {
                BlogId = model.BlogId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("BlogDetail", new { id = model.BlogId });
        }



    }
}
