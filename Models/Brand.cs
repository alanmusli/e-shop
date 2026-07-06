namespace e_store.Models;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string? Location { get; set; } 
    public string? ContactEmail { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    
}