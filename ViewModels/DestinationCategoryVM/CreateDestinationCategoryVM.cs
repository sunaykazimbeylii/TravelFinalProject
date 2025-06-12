using System.ComponentModel.DataAnnotations;

namespace TravelFinalProject.ViewModels
{
    public class CreateDestinationCategoryVM
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
