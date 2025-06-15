using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class Tour : BaseEntity
    {
        [Required, StringLength(200)]
        public string Title { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        [Required]
        public decimal? Price { get; set; }
        public string Duration { get; set; }
        [Required]
        public DateOnly Start_Date { get; set; }
        [Required]
        public DateOnly End_Date { get; set; }
        [Range(0, int.MaxValue)]
        public int Available_seats { get; set; }
        public string Location { get; set; }
        [Required]
        public int? DestinationId { get; set; }
        public Destination? Destination { get; set; }
        public List<Booking>? Bookings { get; set; }
        public List<TourImage>? TourImages { get; set; }

    }
}
