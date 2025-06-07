using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class Destination : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string Image { get; set; }
    }
}
