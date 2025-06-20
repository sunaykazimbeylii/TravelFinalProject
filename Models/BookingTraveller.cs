using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;
using TravelFinalProject.Utilities.Enums;

namespace TravelFinalProject.Models
{
    public class BookingTraveller : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string Nationality { get; set; }  // Ölkə adı və ya kod (ISO code ola bilər)

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public string PassportNumber { get; set; }

    }
}
//booking
//forgot
//myprofile
//rew
