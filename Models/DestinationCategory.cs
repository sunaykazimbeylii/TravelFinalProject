using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class DestinationCategory : BaseEntity
    {

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public List<Destination>? Destinations { get; set; }
    }
}
