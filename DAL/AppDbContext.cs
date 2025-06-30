using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelFinalProject.Models;

namespace TravelFinalProject.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourTranslation> TourTranslations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<DestinationTranslation> DestinationTranslations { get; set; }
        public DbSet<SlideTranslation> SlideTranslations { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<DestinationCategoryTranslation> DestinationCategoryTranslations { get; set; }
        public DbSet<DestinationCategory> DestinationCategories { get; set; }
        public DbSet<DestinationImage> DestinationImages { get; set; }
        public DbSet<TourImage> TourImages { get; set; }
        public DbSet<BookingTravellerTranslation> BookingTravellerTranslations { get; set; }
        public DbSet<BookingTraveller> BookingTravellers { get; set; }

        public DbSet<TravellerPassportNumber> PassportNumbers { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewTranslation> ReviewTranslations { get; set; }
        public DbSet<NotificationSent> NotificationSents { get; set; }
        public DbSet<Currency> Currencies { get; set; }

    }
}
