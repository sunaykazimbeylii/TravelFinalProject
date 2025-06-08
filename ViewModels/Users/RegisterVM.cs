﻿using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels
{
    public class RegisterVM
    {
        [MinLength(3)]
        [MaxLength(50, ErrorMessage = " Name must be 50 characters or fewer")]
        public string Name { get; set; }
        public string Surname { get; set; }
        [MaxLength(100, ErrorMessage = "UserName must be 100 characters or fewer")]
        public string UserName { get; set; }
        [MaxLength(150, ErrorMessage = " Email must be 150 characters or fewer")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        public int PhoneNumber { get; set; }
        public List<Country>? Countries { get; set; }
        public int CountryId { get; set; }
        public DateOnly BirthDate { get; set; }


    }
}
