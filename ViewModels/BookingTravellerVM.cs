using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Utilities.Enums;

namespace TravelFinalProject.ViewModels
{
    public class BookingTravellerVM
    {
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        public string PassportNumber { get; set; } = "";

        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string Nationality { get; set; } = "";
        public int TourId { get; set; }
        public int AdultsCount { get; set; }
        public int ChildrenCount { get; set; }
        public decimal PromoDiscountPercent { get; set; }
    }
}
