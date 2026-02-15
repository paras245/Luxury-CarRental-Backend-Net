using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CarRental_Backend_Net.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Luxury Cars", "Yachts", "SUVs"

        public string Description { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        [JsonIgnore] // Prevent cycles
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
