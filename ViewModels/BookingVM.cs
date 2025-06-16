using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels
{
    public class BookingVM
    {
        public Booking Booking { get; set; } = new Booking();
        [MinLength(1, ErrorMessage = "Azı 1 qonaq əlavə edilməlidir")]
        public List<BookingTraveller> Travellers { get; set; } = new List<BookingTraveller>();
    }
}
