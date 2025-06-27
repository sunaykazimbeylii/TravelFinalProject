using System.ComponentModel.DataAnnotations;
using TravelFinalProject.ViewModels.Users;

namespace TravelFinalProject.ViewModels.ProfileVM
{
    public class ProfileVM
    {
        public IFormFile Photo { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string RegionOrState { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Bio { get; set; }
        public string? StatusMessage { get; set; }
        public ChangePasswordVM ChangePassword { get; set; } = new();
    }
}
