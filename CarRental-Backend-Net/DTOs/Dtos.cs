using Microsoft.AspNetCore.Http;

namespace CarRental_Backend_Net.DTOs
{
    // Auth DTOs (Legacy support)
    public class UserLogin
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Category DTOs
    public class CategoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }

    public class CategoryUpdateDto // No Image update in this simple DTO, typical to separate media update
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Vehicle DTOs
    // Using class for FromForm binding
    public class VehicleCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Car";
        public int CategoryId { get; set; }
        public decimal PricePerDay { get; set; }
        public string Speed { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Offers & Status
        public bool IsOffer { get; set; }
        public double OfferPercentage { get; set; }
        public bool IsTopSelling { get; set; }
        public bool AvailableForRent { get; set; } = true;

        public IFormFile? Image1 { get; set; }
        public IFormFile? Image2 { get; set; }
        public IFormFile? Image3 { get; set; }
    }

    // PATCH DTO - Nullable fields to indicate what to update
    public class VehiclePatchDto
    {
        public decimal? PricePerDay { get; set; }
        public bool? IsOffer { get; set; }
        public double? OfferPercentage { get; set; }
        public bool? IsTopSelling { get; set; }
        public bool? AvailableForRent { get; set; }
    }
}
