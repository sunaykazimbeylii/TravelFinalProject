using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels
{
    public class TourDetailVM
    {
        public int Id { get; set; }
        public Tour Tour { get; set; }
        public List<Tour> RelatedTour { get; set; }
    }
}
