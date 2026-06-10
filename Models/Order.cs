namespace e_store.Models;

public class Order
{
    public int Id { get; set; }
    
    public string? UserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; }
    
}