using System.ComponentModel.DataAnnotations;

namespace API.Data.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Category { get; set; }   
    public string ProductCode { get; set; }
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public decimal Price { get; set; }
    public int MinimumQuantity { get; set; }
    public decimal DiscountRate { get; set; }

}
