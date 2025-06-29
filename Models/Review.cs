using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class Review : BaseEntity
    {
        public int TourId { get; set; }
        public Tour? Tour { get; set; }

        public string UserId { get; set; }
        public AppUser? User { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
