using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class Destination : BaseEntity
    {
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public string Country { get; set; }
        public string Image { get; set; }
        public List<Tour>? Tours { get; set; }
    }
}
