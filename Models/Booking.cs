using TravelFinalProject.Models.Base;
using TravelFinalProject.Utilities;

namespace TravelFinalProject.Models
{
    public class Booking : BaseEntity

    {
        public int UserId { get; set; }
        public int TourId { get; set; }
        public DateTime BookingDate { get; set; }
        public int GuestsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }


    }
}
