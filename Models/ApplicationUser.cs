using Microsoft.AspNetCore.Identity;

namespace e_store.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? StreetAdress { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    
    public Cart? Cart { get; set; }

}