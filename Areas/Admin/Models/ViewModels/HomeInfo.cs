using e_store.Models;

namespace e_store.Areas.Admin.Models.ViewModels;

public class HomeInfo
{
    public int allOrders { get; set; }
    public ICollection<Order> notProccessed { get; set; } = new List<Order>();
    public int productCount { get; set; }
    public int lowCountProds { get; set; }

    public decimal totalSale { get; set; }
    public int messages { get; set; }
}