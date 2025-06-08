﻿using System.ComponentModel.DataAnnotations;
using TravelFinalProject.Models;

namespace TravelFinalProject.ViewModels.TourVM
{
    public class CreateTourVM
    {
        [Required, StringLength(200)]
        public string Title { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public string Duration { get; set; }
        [Required]
        public DateOnly Start_Date { get; set; }
        [Required]
        public DateOnly End_Date { get; set; }
        [Range(0, int.MaxValue)]
        public int Available_seats { get; set; }
        public string Location { get; set; }
        public IFormFile Photo { get; set; }
        [Required]
        public int? DestinationId { get; set; }
        public List<Destination>? Destinations { get; set; }
    }
}
