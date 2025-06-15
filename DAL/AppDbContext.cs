using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.Models;

namespace TravelFinalProject.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<DestinationCategory> DestinationCategories { get; set; }
        public DbSet<DestinationImage> DestinationImages { get; set; }
        public DbSet<TourImage> TourImages { get; set; }
        public DbSet<BookingTraveller> BookingTravellers { get; set; }
    }
}
