using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels
{
    public class TourListPageVM
    {
        public PaginatedVM<GetTourVM> PaginatedTours { get; set; }
        public List<Destination> Destinations { get; set; }
        public TourSearchVM SearchForm { get; set; }
        public string SelectedCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
