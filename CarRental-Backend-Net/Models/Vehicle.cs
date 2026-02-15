using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental_Backend_Net.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Ferrari F8", "Sunseeker 88"

        [Required]
        public string Type { get; set; } = "Car"; // Car, Yacht, Bike, etc.

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public decimal PricePerDay { get; set; } // Base Price

        // Offer Logic
        public bool IsOffer { get; set; }
        public double OfferPercentage { get; set; } // e.g., 20 for 20% off
        
        // Calculated property for Display
        public decimal DiscountedPrice => IsOffer && OfferPercentage > 0 
            ? PricePerDay - (PricePerDay * (decimal)(OfferPercentage / 100)) 
            : PricePerDay;

        // Specs
        public string Speed { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        
        // Status
        public bool IsTopSelling { get; set; } // "Popular"
        public bool AvailableForRent { get; set; } = true;

        // Media
        public string ImagePaths { get; set; } = "[]"; // JSON array
    }
}
