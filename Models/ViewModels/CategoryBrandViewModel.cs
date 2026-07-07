namespace e_store.Models;

public class CategoryBrandViewModel
{
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Brand> Brands { get; set; } = new List<Brand>();
}