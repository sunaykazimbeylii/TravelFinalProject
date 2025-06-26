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

        public decimal TotalPrice { get; set; }

        public int Adults { get; set; }
        public int Children { get; set; }

        public int TravellerCount => Adults + Children;

        public BookingStatus Status { get; set; }

        public List<string> PassportNumbers { get; set; } = new();

        public Booking? Booking { get; set; }
    }

}
