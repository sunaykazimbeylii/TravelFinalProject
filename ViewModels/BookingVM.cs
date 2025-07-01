using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models;
using TravelFinalProject.Utilities;
using TravelFinalProject.Utilities.Enums;

namespace TravelFinalProject.ViewModels
{
    public class BookingVM
    {
        public int TourId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public Gender Gender { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        public string Nationality { get; set; }
        public string? BookingForUserId { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PromoDiscountPercent { get; set; } = 0;
        public int Adults { get; set; }
        public int Children { get; set; }
        public BookingStatus Status { get; set; }
        public decimal PricePerAdult { get; set; }
        public decimal PricePerChild { get; set; }
        public int AdultsCount { get; set; }
        public int GuestsCount { get; set; }
        public int ChildrenCount { get; set; }

        public decimal Subtotal => (AdultsCount * PricePerAdult) + (ChildrenCount * PricePerChild);
        public decimal DiscountAmount => PromoDiscountPercent > 0 ? Subtotal * PromoDiscountPercent / 100 : 0;
        public decimal Total => Subtotal - DiscountAmount;
        public List<string> PassportNumbers { get; set; } = new();

        public Booking? Booking { get; set; }

        public List<BookingTravellerVM> Guests { get; set; } = new();
    }

}
