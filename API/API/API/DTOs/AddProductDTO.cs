using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class AddProductDTO
    {
        [Required]
        public IFormFile ImageSource {  get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string ProductCode { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int MinimumQuantity { get; set; }
        [Required]
        public decimal DiscountRate { get; set; }
    }
}
