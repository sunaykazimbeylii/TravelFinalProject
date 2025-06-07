using TravelFinalProject.Models.Base;

namespace TravelFinalProject.Models
{
    public class Tour : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public DateOnly Start_Date { get; set; }
        public DateOnly End_Date { get; set; }
        public int Available_seats { get; set; }
        public string Location { get; set; }
        public string Image { get; set; }
        public int DestinationId { get; set; }

    }
}
