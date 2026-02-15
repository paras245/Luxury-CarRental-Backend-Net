using System.ComponentModel.DataAnnotations;

namespace CarRental_Backend_Net.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // SUV, Luxury, Sports

        public decimal Price { get; set; }

        public string Speed { get; set; } = string.Empty; // e.g., "0-100km/h in 2.9s"

        public string Description { get; set; } = string.Empty;

        public bool IsPopular { get; set; }

        public bool IsOffer { get; set; }
        
        public bool AvailableForRent { get; set; } = true;

        // Stores a comma-separated list of image paths or a JSON string
        public string ImagePaths { get; set; } = string.Empty; 
    }
}
