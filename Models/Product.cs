using System.ComponentModel.DataAnnotations;

namespace e_store.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public string? Description { get; set; }
    public decimal Price  { get; set; }
    public int InventoryCount  { get; set; }
    public string? ImageUrl  { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    [Required]
    [StringLength(20)]
    public string SKU { get; set; }
    
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    

}