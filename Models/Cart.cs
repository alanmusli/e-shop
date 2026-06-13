namespace e_store.Models;

public class Cart
{
    public int Id { get; set; }
    
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public string? SessionId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();    
}