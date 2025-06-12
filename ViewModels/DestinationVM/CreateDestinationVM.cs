using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels
{
    public class CreateDestinationVM
    {
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public string Country { get; set; }
        public IFormFile Photo { get; set; }
        public List<Tour>? Tours { get; set; }
    }
}
