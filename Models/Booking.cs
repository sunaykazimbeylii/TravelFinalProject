using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;
using TravelFinalProject.Utilities;

namespace TravelFinalProject.Models
{
    public class Booking : BaseEntity

    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int TourId { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [Range(1, 100)]
        public int GuestsCount { get; set; }
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public Tour? Tour { get; set; }


    }
}
